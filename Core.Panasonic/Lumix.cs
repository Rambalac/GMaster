namespace GMaster.Core.Camera.Panasonic
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using LumixData;
    using Network;
    using Tools;

    public partial class Lumix : IDisposable
    {
        private float lastOldPinchSize;

        public CameraParser Parser { get; private set; }

        [RunnableAction(MethodGroup.Capture)]
        public async Task<bool> CaptureStart()
        {
            ReportAction();
            return await Try(async () =>
            {
                await http.Get<BaseRequestResult>("?mode=camcmd&value=capture");
                return true;
            });
        }

        [RunnableAction(MethodGroup.Capture)]
        public async Task<bool> CaptureStop()
        {
            ReportAction();
            return await Try(async () =>
            {
                await http.Get<BaseRequestResult>("?mode=camcmd&value=capture_cancel");
                return true;
            });
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> ChangeFocus(ChangeDirection focusDirection)
        {
            ReportAction(focusDirection);
            return await Try(async () =>
            {
                var focus = await http.GetString("?mode=camctrl&type=focus&value=" + focusDirection.GetString());

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
            });
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> ChangeZoom(ChangeDirection zoomDirection)
        {
            ReportAction(zoomDirection);
            return await Try(async () => await http.Get<BaseRequestResult>("?mode=camcmd&value=" + zoomDirection.GetString()));
        }

        public async Task<bool> Connect(int liveviewport, string lang)
        {
            language = lang;
            var token = connectCancellation.Token;
            var connectStage = 0;
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
                            await TryGet("?mode=setsetting&type=device_name&value=SM-G9350");
                        }

                        connectStage = 2;

                        LumixState.Reset();

                        LumixState.MenuSet = await GetMenuSet();
                        if (LumixState.MenuSet == null)
                        {
                            LogError($"Camera connection failed on Stage {connectStage} for camera {Device.ModelName}", "CameraConnect");
                            LumixState.IsLimited = true;
                        }

                        if (!LumixState.IsLimited)
                        {
                            LumixState.CurMenu = await GetCurMenu();
                        }

                        connectStage = 3;
                        await SwitchToRec();

                        connectStage = 4;
                        LumixState.LensInfo = await GetLensInfo();

                        connectStage = 5;
                        LumixState.State = await GetState();

                        connectStage = 6;
                        await http.Get<BaseRequestResult>($"?mode=startstream&value={liveviewport}", token);

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
                    catch (OperationCanceledException)
                    {
                        return false;
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
                return true;
            }
            catch (OperationCanceledException)
            {
                connectCancellation.Dispose();
                return false;
            }
            catch (Exception e)
            {
                LogError($"Camera connection failed on Stage {connectStage} for camera {Device.ModelName}", e);
                return false;
            }
            finally
            {
                isConnecting = false;
            }
        }

        public void Disconnect()
        {
            try
            {
                connectCancellation.Cancel();
                connectCancellation = new CancellationTokenSource();

                stateTimer.Change(-1, -1);
                Debug.WriteLine("Timer stopped", "Disconnect");
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> FocusPointMove(FloatPoint p)
        {
            ReportAction(p);
            return await OldNewAction(
                async () => await TryGet($"?mode=camctrl&type=touch&value={(int)(p.X * 1000)}/{(int)(p.Y * 1000)}&value2=on"),
                async () => await TryGet($"?mode=camctrl&type=touchaf&value={(int)(p.X * 1000)}/{(int)(p.Y * 1000)}"),
                Profile.NewTouch,
                f => Profile.NewTouch = f);
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> FocusPointResize(PinchStage stage, FloatPoint p, float size)
        {
            ReportAction(size);
            return await OldNewAction(
                async () => await PinchZoom(stage, p, size),
                async () =>
                {
                    var dif = size - lastOldPinchSize;
                    if (Math.Abs(dif) > 0.03f)
                    {
                        lastOldPinchSize = size;
                        var val = dif > 0 ? "up" : "down";
                        await http.Get<BaseRequestResult>($"?mode=camctrl&type=touchaf_chg_area&value={val}");
                    }
                    if (stage == PinchStage.Stop || stage == PinchStage.Single)
                    {
                        lastOldPinchSize = 0;
                    }

                    return true;
                },
                Profile.NewTouch,
                f => Profile.NewTouch = f);
        }

        public async Task<FocusMode> GetFocusMode()
        {
            try
            {
                var result = await http.Get<FocusModeRequestResult>("?mode=getsetting&type=focusmode");
                return result.Value.FocusMode;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return FocusMode.Unknown;
            }
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> MfAssistAf()
        {
            ReportAction();
            if (Profile.ManualFocusAF)
            {
                return await Try(async () =>
                {
                    try
                    {
                        await http.Get<BaseRequestResult>("?mode=camcmd&value=oneshot_af");
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
                });
            }

            return false;
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> MfAssistMove(PinchStage stage, FloatPoint p)
        {
            ReportAction(stage, p);
            return await OldNewAction(
                async () => await NewMfAssistMove(stage, p),
                async () => await TryGetString($"?mode=camctrl&type=mf_asst&value={(int)(p.X * 1000)}/{(int)(p.Y * 1000)}"),
                Profile.NewTouch,
                f => Profile.NewTouch = f);
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> MfAssistOff()
        {
            ReportAction();
            return await OldNewAction(
            async () => await TryGetString("?mode=camctrl&type=asst_disp&value=off&value2=mf_asst/0/0"),
            async () => await TryGet("?mode=setsetting&type=mf_asst_mag&value=1"),
            Profile.NewTouch,
            f => Profile.NewTouch = f);
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> MfAssistPinp(bool pInP)
        {
            ReportAction(pInP);
            var val = pInP ? "pinp" : "full";
            return await TryGetString($"?mode=camctrl&type=asst_disp&value={val}&value2=mf_asst/0/0");
        }

        [RunnableAction(MethodGroup.Focus)]
        public async Task<bool> MfAssistZoom(PinchStage stage, FloatPoint p, float size)
        {
            ReportAction(stage, p, size);
            return await OldNewAction(
                async () => await PinchZoom(stage, p, size),
                async () => await OldMfAssistZoom(stage, p, size),
                Profile.NewTouch,
                f => Profile.NewTouch = f);
        }

        [RunnableAction(MethodGroup.Capture)]
        public async Task<bool> RecStart()
        {
            ReportAction();
            return await Try(async () =>
            {
                await http.Get<BaseRequestResult>("?mode=camcmd&value=video_recstart");
                if (Profile.RecStop)
                {
                    LumixState.RecState = RecState.Unknown;
                    await Task.Delay(100);
                    LumixState.State = await GetState();
                    return true;
                }
                else
                {
                    LumixState.RecState = RecState.StopNotSupported;
                }

                return true;
            });
        }

        [RunnableAction(MethodGroup.Capture)]
        public async Task<bool> RecStop()
        {
            ReportAction();
            if (Profile.RecStop)
            {
                try
                {
                    await http.Get<BaseRequestResult>("?mode=camcmd&value=video_recstop");
                    await Task.Delay(500);
                    LumixState.State = await GetState();
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
        public async Task<bool> ReleaseTouchAF()
        {
            ReportAction();
            return await TryGet("?mode=camcmd&value=touchafrelease");
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
        public async Task<bool> SendMenuItem(ICameraMenuItem value)
        {
            ReportAction(value);
            if (value != null)
            {
                return await Try(async () =>
                    await http.Get<BaseRequestResult>(new Dictionary<string, string>
                    {
                        { "mode", value.Command },
                        { "type", value.CommandType },
                        { "value", value.Value }
                    }));
            }

            return false;
        }

        public async Task StopStream()
        {
            if (!isConnecting)
            {
                await Try(async () => await http.Get<BaseRequestResult>("?mode=stopstream"));
            }
        }

        public async Task<bool> SwitchToRec()
        {
            return await Try(async () => await http.Get<BaseRequestResult>("?mode=camcmd&value=recmode"));
        }

        private async Task<CurMenu> GetCurMenu()
        {
            var curmenuString = await http.GetString("?mode=getinfo&type=curmenu");
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

        private async Task<LensInfo> GetLensInfo()
        {
            string raw = null;
            try
            {
                raw = await http.GetString("?mode=getinfo&type=lens");
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

        private async Task<MenuSet> GetMenuSet()
        {
            var allmenuString = await http.GetString("?mode=getinfo&type=allmenu");
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

        private async Task<CameraState> GetState()
        {
            var response = await http.Get<CameraStateRequestResult>("?mode=getstate");
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
        private async Task<bool> NewMfAssistMove(PinchStage stage, FloatPoint p)
        {
            ReportAction(stage, p);
            if (!autoreviewUnlocked)
            {
                autoreviewUnlocked = true;
                await TryGet("?mode=camcmd&value=autoreviewunlock");
            }

            var val2 = $"{(int)(p.X * 1000)}/{(int)(p.Y * 1000)}";
            if (stage != PinchStage.Single)
            {
                var res = await TryGetString($"?mode=camctrl&type=touch_trace&value={stage.GetString()}&value2={val2}");
                if (stage == PinchStage.Stop)
                {
                    autoreviewUnlocked = false;
                }

                return res;
            }

            await TryGetString($"?mode=camctrl&type=touch_trace&value=start&value2={val2}");
            await TryGetString($"?mode=camctrl&type=touch_trace&value=continue&value2={val2}");
            await TryGetString($"?mode=camctrl&type=touch_trace&value=stop&value2={val2}");
            autoreviewUnlocked = false;
            return true;
        }

        private async Task<bool> OldMfAssistZoom(PinchStage stage, FloatPoint floatPoint, float size)
        {
            if (stage == PinchStage.Start)
            {
                lastOldPinchSize = size;
                return true;
            }

            if (LumixState.CameraMode != CameraMode.MFAssist)
            {
                return await MfAssistMove(stage, floatPoint);
            }

            var val = size - lastOldPinchSize > 0 ? 10 : 5;
            Debug.WriteLine("Mag val:" + val, "MFAssist");
            return await TryGet($"?mode=setsetting&type=mf_asst_mag&value={val}");
        }

        private async Task<bool> OldNewAction(Func<Task<bool>> newAction, Func<Task<bool>> oldAction, bool flag, Action<bool> flagSet)
        {
            return await Try(async () =>
            {
                if (flag)
                {
                    try
                    {
                        return await newAction();
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

                return await oldAction();
            });
        }

        [RunnableAction(MethodGroup.Focus)]
        private async Task<bool> PinchZoom(PinchStage stage, FloatPoint p, float size)
        {
            try
            {
                if (!autoreviewUnlocked)
                {
                    autoreviewUnlocked = true;
                    await TryGet("?mode=camcmd&value=autoreviewunlock");
                }

                var pp1 = new IntPoint(p - size, 1000f).Clamp(0, 1000);
                var pp2 = new IntPoint(p + size, 1000f).Clamp(0, 1000);

                var url = $"?mode=camctrl&type=pinch&value={stage.GetString()}&value2={pp1.X}/{pp1.Y}/{pp2.X}/{pp2.Y}";
                Debug.WriteLine(url, "PinchZoom");
                var resstring = await http.GetString(url);
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