 using PermissionValidator;

 partial class Program
 {
        static void Main(string[] args)
        {
            var service = new MyService();
            var myServiceProxy = new MyServiceProxy(service);

            myServiceProxy.EasyMethod2();

            myServiceProxy.EasyMethod();

            MyService2 myService = new MyService2();
            var myService2Proxy = new MyService2Proxy(myService);
            Console.WriteLine(myService2Proxy.func1());

            //service.EasyMethod();
        }
 }  