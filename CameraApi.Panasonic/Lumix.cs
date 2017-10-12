namespace CameraApi.Panasonic
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using CameraApi.Core;
    using CameraApi.Panasonic.LumixData;
    using GMaster.Core.Network;
    using GMaster.Core.Tools;

    public partial class Lumix : ILiveviewProvider, IUdpCamera
    {
        private CancellationTokenSource connectCancellation;

        public Lumix(string lang)
        {
            language = lang;
        }

        private float lastOldPinchSize;

        public CameraParser Parser { get; private set; }

        [RunnableAction(MethodGroup.Capture)]
        public async Task<bool> CaptureStart(CancellationToken token)
        {
            ReportAction();
            return await TryGet("?mode=camcmd&value=capture", token);
        }

        [RunnableAction(MethodGroup.Capture)]
        public async Task<bool> CaptureStop(CancellationToken token)
        {
            ReportAction();
            return await TryGet("?mode=camcmd&value=capture_cancel", token);
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> ChangeFocus(ChangeDirection focusDirection, CancellationToken token)
        {
            ReportAction(focusDirection);
            return await Try(
                async cancel =>
                {
                    var focus = await http.GetString("?mode=camctrl&type=focus&value=" + focusDirection.GetString(), cancel);

                    var fp = Parser.ParseFocus(focus);
                    if (fp == null)
                    {
                        return false;
                    }

                    if (fp.Maximum != LumixState.MaximumFocus)
                    {
                        LumixState.MaximumFocus = fp.Maximum;
                    }

                    if (fp.Value != LumixState.CurrentFocus)
                    {
                        LumixState.CurrentFocus = fp.Value;
                    }

                    return true;
                }, token);
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> ChangeZoom(ChangeDirection zoomDirection, CancellationToken token)
        {
            ReportAction(zoomDirection);
            return await TryGet("?mode=camcmd&value=" + zoomDirection.GetString(), token);
        }

        public async Task StartLiveview(LiveviewReceiver receiver, CancellationToken cancellation)
        {
            if (!isLiveview)
            {
                isLiveview = true;
                const int liveviewport = 23456;
                await http.Get<BaseRequestResult>($"?mode=startstream&value={liveviewport}", cancellation);
            }
        }

        public async Task Connect(CancellationToken cancel)
        {

            var connectStage = 0;

            connectCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancel);
            var token = connectCancellation.Token;

            try
            {
                LogTrace("Connecting camera " + Device.ModelName);
                do
                {
                    try
                    {
                        if (Profile.RequestConnection)
                        {
                            await RequestAccess(token);
                        }

                        connectStage = 1;

                        if (Profile.SetDeviceName)
                        {
                            await TryGet("?mode=setsetting&type=device_name&value=SM-G9350", token);
                        }

                        connectStage = 2;

                        LumixState.Reset();

                        LumixState.MenuSet = await GetMenuSet(token);
                        if (LumixState.MenuSet == null)
                        {
                            LogError($"Camera connection failed on Stage {connectStage} for camera {Device.ModelName}", "CameraConnect");
                            LumixState.IsLimited = true;
                        }

                        if (!LumixState.IsLimited)
                        {
                            LumixState.CurMenu = await GetCurMenu(token);
                        }

                        connectStage = 3;
                        await SwitchToRec(token);

                        connectStage = 4;
                        LumixState.LensInfo = await GetLensInfo(token);

                        connectStage = 5;
                        LumixState.State = await GetState(token);

                        token.ThrowIfCancellationRequested();
                        break;
                    }
                    catch (ConnectionLostException)
                    {
                        Debug.WriteLine("Connection lost", "Connection");
                    }
                    catch (TimeoutException)
                    {
                        Debug.WriteLine("Timeout", "Connection");
                    }
                    catch (Exception ex)
                    {
                        LogError($"Camera connection failed on Stage {connectStage} for camera {Device.ModelName}", ex);
                    }

                    await Task.Delay(1000, token);
                }
                while (true);

                if (OffFrameProcessor == null)
                {
                    connectStage = 7;
                    OffFrameProcessor = new OffFrameProcessor(Device.ModelName, Parser, LumixState);
                    OffFrameProcessor.LensChanged += OffFrameProcessor_LensChanged;
                }

                stateTimer.Change(2000, 2000);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                LogError($"Camera connection failed on Stage {connectStage} for camera {Device.ModelName}", e);
                throw;
            }
            finally
            {
                var temp = connectCancellation;
                connectCancellation = null;
                temp.Dispose();
            }
        }

        public async Task Disconnect(CancellationToken token)
        {
            try
            {
                connectCancellation?.Cancel();

                stateTimer.Change(-1, -1);
                Debug.WriteLine("Timer stopped", "Disconnect");

                await StopLiveview(token);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> FocusPointMove(FloatPoint p, CancellationToken cancel)
        {
            ReportAction(p);
            var point = $"{(int)(p.X * 1000)}/{(int)(p.Y * 1000)}";
            return await OldNewAction(
                async c => await TryGet($"?mode=camctrl&type=touch&value={point}&value2=on", c),
                async c => await TryGet($"?mode=camctrl&type=touchaf&value={point}", c),
                Profile.NewTouch,
                f => Profile.NewTouch = f,
                cancel);
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> FocusPointResize(PinchStage stage, FloatPoint p, float size, CancellationToken cancel)
        {
            ReportAction(size);
            return await OldNewAction(
                async c => await PinchZoom(stage, p, size, c),
                async c =>
                {
                    var dif = size - lastOldPinchSize;
                    if (Math.Abs(dif) > 0.03f)
                    {
                        lastOldPinchSize = size;
                        var val = dif > 0 ? "up" : "down";
                        await http.Get<BaseRequestResult>($"?mode=camctrl&type=touchaf_chg_area&value={val}", c);
                    }
                    if (stage == PinchStage.Stop || stage == PinchStage.Single)
                    {
                        lastOldPinchSize = 0;
                    }

                    return true;
                },
                Profile.NewTouch,
                f => Profile.NewTouch = f,
                cancel);
        }

        private readonly IDictionary<LumixFocusMode, FocusMode> ToFocusMode = new Dictionary<LumixFocusMode, FocusMode>
        {
            { LumixFocusMode.MF, FocusMode.MF },
            { LumixFocusMode.AFC, FocusMode.AFC },
            { LumixFocusMode.AFF, FocusMode.AFF },
            { LumixFocusMode.AFS, FocusMode.AFS },
            { LumixFocusMode.Unknown, FocusMode.Unknown }
        };

        private bool isLiveview;

        public async Task<FocusMode> GetFocusMode(CancellationToken cancel)
        {
            try
            {
                var result = await http.Get<FocusModeRequestResult>("?mode=getsetting&type=focusmode", cancel);
                return ToFocusMode[result.Value.FocusMode];
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return ToFocusMode[LumixFocusMode.Unknown];
            }
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> MfAssistAf(CancellationToken cancel)
        {
            ReportAction();
            if (Profile.ManualFocusAF)
            {
                return await Try(
                    async c =>
                    {
                        try
                        {
                            await http.Get<BaseRequestResult>("?mode=camcmd&value=oneshot_af", c);
                            return true;
                        }
                        catch (LumixException ex)
                        {
                            if (ex.Error == LumixError.ErrorParam)
                            {
                                Profile.ManualFocusAF = false;
                                ProfileUpdated?.Invoke();
                                return false;
                            }
                            throw;
                        }
                    }, cancel);
            }

            return false;
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> MfAssistMove(PinchStage stage, FloatPoint p, CancellationToken cancel)
        {
            ReportAction(stage, p);
            return await OldNewAction(
                async c => await NewMfAssistMove(stage, p, c),
                async c => await TryGetString($"?mode=camctrl&type=mf_asst&value={(int)(p.X * 1000)}/{(int)(p.Y * 1000)}", c),
                Profile.NewTouch,
                f => Profile.NewTouch = f,
                cancel);
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> MfAssistOff(CancellationToken cancel)
        {
            ReportAction();
            return await OldNewAction(
            async c => await TryGetString("?mode=camctrl&type=asst_disp&value=off&value2=mf_asst/0/0", c),
            async c => await TryGet("?mode=setsetting&type=mf_asst_mag&value=1", c),
            Profile.NewTouch,
            f => Profile.NewTouch = f,
            cancel);
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> MfAssistPinp(bool pInP, CancellationToken cancel)
        {
            ReportAction(pInP);
            var val = pInP ? "pinp" : "full";
            return await TryGetString($"?mode=camctrl&type=asst_disp&value={val}&value2=mf_asst/0/0", cancel);
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> MfAssistZoom(PinchStage stage, FloatPoint p, float size, CancellationToken cancel)
        {
            ReportAction(stage, p, size);
            return await OldNewAction(
                async c => await PinchZoom(stage, p, size, c),
                async c => await OldMfAssistZoom(stage, p, size, c),
                Profile.NewTouch,
                f => Profile.NewTouch = f,
                cancel);
        }

        [RunnableAction(MethodGroup.Capture)]
        public async Task<bool> RecStart(CancellationToken cancel)
        {
            ReportAction();
            return await Try(
                async c =>
                {
                    await http.Get<BaseRequestResult>("?mode=camcmd&value=video_recstart", c);
                    if (Profile.RecStop)
                    {
                        LumixState.RecState = RecState.Unknown;
                        await Task.Delay(100);
                        LumixState.State = await GetState(c);
                        return true;
                    }
                    else
                    {
                        LumixState.RecState = RecState.StopNotSupported;
                    }

                    return true;
                }, cancel);
        }

        [RunnableAction(MethodGroup.Capture)]
        public async Task<bool> RecStop(CancellationToken cancel)
        {
            ReportAction();
            if (Profile.RecStop)
            {
                try
                {
                    await http.Get<BaseRequestResult>("?mode=camcmd&value=video_recstop", cancel);
                    await Task.Delay(500);
                    LumixState.State = await GetState(cancel);
                    return true;
                }
                catch (LumixException ex)
                {
                    if (ex.Error == LumixError.ErrorParam)
                    {
                        Debug.WriteLine("RecStop not supported", "RecStop");
                        Profile.RecStop = false;
                        LumixState.RecState = RecState.StopNotSupported;
                    }
                    else
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }

            return false;
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> ReleaseTouchAF(CancellationToken cancel)
        {
            ReportAction();
            return await TryGet("?mode=camcmd&value=touchafrelease", cancel);
        }

        public async Task RunCommand(string methodName, object[] prm)
        {
            if (!reportingAction)
            {
                try
                {
                    reportingAction = true;
                    if (!RunnableCommands.TryGetValue(methodName, out var command))
                    {
                        throw new ArgumentException("Wrong command: " + methodName);
                    }

                    if (command.Async)
                    {
                        await (Task)command.Method.Invoke(this, prm);
                    }
                    else
                    {
                        command.Method.Invoke(this, prm);
                    }
                }
                finally
                {
                    reportingAction = false;
                }
            }
        }

        [RunnableAction(MethodGroup.Properties)]
        public async Task<bool> SendMenuItem(ICameraMenuItem value, CancellationToken cancel)
        {
            ReportAction(value);
            if (value != null)
            {
                return await TryGet(
                    new Dictionary<string, string>
                    {
                        { "mode", value.Command },
                        { "type", value.CommandType },
                        { "value", value.Value }
                    }, cancel);
            }

            return false;
        }

        public async Task StopLiveview(CancellationToken token)
        {
            if (isLiveview)
            {
                await Try(async cancel => await http.Get<BaseRequestResult>("?mode=stopstream", cancel), token);
            }
        }

        public async Task<bool> SwitchToRec(CancellationToken cancel)
        {
            return await TryGet("?mode=camcmd&value=recmode", cancel);
        }

        private async Task<CurMenu> GetCurMenu(CancellationToken cancel)
        {
            var curmenuString = await http.GetString("?mode=getinfo&type=curmenu", cancel);
            var response = Http.ReadResponse<CurMenuRequestResult>(curmenuString);

            if (response.MenuInfo == null)
            {
                return null;
            }

            try
            {
                var result = Parser.ParseCurMenu(response.MenuInfo);
                return result;
            }
            catch (AggregateException)
            {
                LogError("Cannot parse CurMenu", (object)curmenuString);
                return null;
            }
        }

        private async Task<LensInfo> GetLensInfo(CancellationToken cancel)
        {
            string raw = null;
            try
            {
                raw = await http.GetString("?mode=getinfo&type=lens", cancel);
                return Parser.ParseLensInfo(raw);
            }
            catch (Exception)
            {
                if (CheckAlreadyConnected(raw))
                {
                    return null;
                }

                Debug.WriteLine("LensInfo: " + raw, "LensInfo");
                throw;
            }
        }

        private async Task<MenuSet> GetMenuSet(CancellationToken cancel)
        {
            var allmenuString = await http.GetString("?mode=getinfo&type=allmenu", cancel);
            var result = Http.ReadResponse<MenuSetRequestResult>(allmenuString);

            if (result.MenuSet == null)
            {
                return null;
            }

            try
            {
                if (Parser == null)
                {
                    Parser = CameraParser.TryParseMenuSet(result.MenuSet, language, out var menuset);
                    return menuset;
                }

                return Parser.ParseMenuSet(result.MenuSet, language);
            }
            catch (AggregateException)
            {
                LogError("Cannot parse MenuSet", (object)allmenuString);
                return null;
            }
        }

        private async Task<CameraState> GetState(CancellationToken cancel)
        {
            var response = await http.Get<CameraStateRequestResult>("?mode=getstate", cancel);
            var newState = response.State;
            if (newState.Rec == OnOff.On)
            {
                LumixState.RecState = Profile.RecStop ? RecState.Started : RecState.StopNotSupported;
            }
            else
            {
                LumixState.RecState = RecState.Stopped;
            }

            return newState;
        }

        [RunnableAction(MethodGroup.Focus)]
        private async Task<bool> NewMfAssistMove(PinchStage stage, FloatPoint p, CancellationToken cancel)
        {
            ReportAction(stage, p);
            if (!autoreviewUnlocked)
            {
                autoreviewUnlocked = true;
                await TryGet("?mode=camcmd&value=autoreviewunlock", cancel);
            }

            var val2 = $"{(int)(p.X * 1000)}/{(int)(p.Y * 1000)}";
            if (stage != PinchStage.Single)
            {
                var res = await TryGetString($"?mode=camctrl&type=touch_trace&value={stage.GetString()}&value2={val2}", cancel);
                if (stage == PinchStage.Stop)
                {
                    autoreviewUnlocked = false;
                }

                return res;
            }

            await TryGetString($"?mode=camctrl&type=touch_trace&value=start&value2={val2}", cancel);
            await TryGetString($"?mode=camctrl&type=touch_trace&value=continue&value2={val2}", cancel);
            await TryGetString($"?mode=camctrl&type=touch_trace&value=stop&value2={val2}", cancel);
            autoreviewUnlocked = false;
            return true;
        }

        private async Task<bool> OldMfAssistZoom(PinchStage stage, FloatPoint floatPoint, float size, CancellationToken cancel)
        {
            if (stage == PinchStage.Start)
            {
                lastOldPinchSize = size;
                return true;
            }

            if (LumixState.LumixCameraMode != LumixCameraMode.MFAssist)
            {
                return await MfAssistMove(stage, floatPoint, cancel);
            }

            var val = size - lastOldPinchSize > 0 ? 10 : 5;
            Debug.WriteLine("Mag val:" + val, "MFAssist");
            return await TryGet($"?mode=setsetting&type=mf_asst_mag&value={val}", cancel);
        }

        private async Task<bool> OldNewAction(
            Func<CancellationToken, Task<bool>> newAction,
            Func<CancellationToken, Task<bool>> oldAction,
            bool flag,
            Action<bool> flagSet,
            CancellationToken cancel)
        {
            return await Try(
                async c =>
                {
                    if (flag)
                    {
                        try
                        {
                            return await newAction(c);
                        }
                        catch (LumixException ex)
                        {
                            if (ex.Error == LumixError.ErrorParam)
                            {
                                LogTrace("New action not supported", "NewAction");
                                flagSet(false);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }

                    return await oldAction(c);
                }, cancel);
        }

        [RunnableAction(MethodGroup.Focus)]
        private async Task<bool> PinchZoom(PinchStage stage, FloatPoint p, float size, CancellationToken cancel)
        {
            try
            {
                if (!autoreviewUnlocked)
                {
                    autoreviewUnlocked = true;
                    await TryGet("?mode=camcmd&value=autoreviewunlock", cancel);
                }

                var pp1 = new IntPoint(p - size, 1000f).Clamp(0, 1000);
                var pp2 = new IntPoint(p + size, 1000f).Clamp(0, 1000);

                var url = $"?mode=camctrl&type=pinch&value={stage.GetString()}&value2={pp1.X}/{pp1.Y}/{pp2.X}/{pp2.Y}";
                Debug.WriteLine(url, "PinchZoom");
                var resstring = await http.GetString(url, cancel);
                if (resstring.StartsWith("<xml>"))
                {
                    return false;
                }

                var csv = resstring.Split(',');
                if (csv[0] != "ok")
                {
                    return false;
                }

                if (stage == PinchStage.Stop)
                {
                    autoreviewUnlocked = false;
                }

                return true;
            }
            catch (LumixException)
            {
                throw;
            }
            catch (ConnectionLostException)
            {
                Debug.WriteLine("Connection lost", "Connection");
                return false;
            }
            catch (Exception ex)
            {
                LogError("Camera action failed", ex);
                return false;
            }
        }
    }
}