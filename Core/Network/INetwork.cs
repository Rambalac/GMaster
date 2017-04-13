namespace GMaster.Core.Network
{
    using System;
    using System.Collections.Generic;

    public interface INetwork
    {
        event Action NetworkStatusChanged;

        IDatagramSocket CreateDatagramSocket();

        IEnumerable<string> GetHostNames();
    }
}