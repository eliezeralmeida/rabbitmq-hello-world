/*
Our code is still pretty simplistic and doesn't try to solve more complex (but important) problems, like:

How should the client react if there are no servers running?
Should a client have some kind of timeout for the RPC?
If the server malfunctions and raises an exception, should it be forwarded to the client?
Protecting against invalid incoming messages (eg checking bounds, type) before processing.
*/

using ClientRpc;

Console.WriteLine("RPC Client");
var n = args.Length > 0 ? args[0] : throw new ArgumentException("Arg1 is required");

RequestAsync(n).Wait();
Request(n);

static async Task RequestAsync(string param)
{
    Console.WriteLine($" [x] Conecting on Server (RequestAsync)");
    var rpc = new RpcClientGithubExample();

    Console.WriteLine($" [x] Requesting fibbonacci for {param}");
    var response = await rpc.CallAsync(param);
    Console.WriteLine($" [.] Got '{response}'");
}

static void Request(string param)
{
    Console.WriteLine($" [x] Conecting on Server (Request)");
    var rpc = new RpcClientTutorialExample();

    Console.WriteLine($" [x] Requesting fibbonacci for {param}");
    var response = rpc.Call(param);
    Console.WriteLine($" [.] Got '{response}'");

    rpc.Close();
}