using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MsgPack;
using MsgPack.Rpc.Client;

namespace mGCS
{
    class AirSimHelper
    {
        RpcClientConfiguration configuration = new RpcClientConfiguration();
        // Tweak configuration here...
        //using ( dynamic proxy = DynamicRpcProxy.Create( new DnsEndPoint( "example.com", 19860 ), configuration ) )
        //{
        //    proxy.Foo(...); // Dynamic dispatch!
        //}
     }
}
