namespace GMaster.Core.Network
{
    using System;
    using System.Collections.Generic;

    public interface INetwork
    {
        IDatagramSocket CreateDatagramSocket();

        event Action NetworkStatusChanged;
        IEnumerable<string> GetHostNames();
    }
}