using System;
using System.Threading;
using System.Security.Cryptography;

namespace nonceCalc
{
    /// <summary>
    /// The handler for the nonceFound event
    /// </summary>
    /// <param name="nonce">nonce that gives the result</param>
    /// <param name="result">result, that was found</param>
    public delegate void NonceFoundHandler(ulong nonce,ulong result);

    class Program
    {
        /// <summary>
        /// Fired, when a nonce was found
        /// </summary>
        public static event NonceFoundHandler nonceFound;

        /// <summary>
        /// Continue execution if true.
        /// This cancels calculation if false.
        /// </summary>
        public static bool cont = false;

        /// <summary>
        /// Row of threads
        /// </summary>
        private static Thread[] T;

        /// <summary>
        /// Date/Time when calculation started
        /// </summary>
        private static DateTime DT;

        /// <summary>
        /// Row of nonces (same size as threads)
        /// </summary>
        public static ulong[] nonce;

        static void Main(string[] args)
        {
            int length;
            int pow_addbytes;
            int pow;

            Console.Clear();
            Console.WriteLine("POW Benchmark");
            Console.WriteLine("This tool allows you to modify the POW conditions");
            Console.WriteLine();
            Console.WriteLine("How POW is done");
            Console.WriteLine("POW is a calculation that gives back a result. This result must");
            Console.WriteLine("be smaller than the result of the folowing equation:");
            PrintColor.printColorL("Long Form:  target = ulong.MaxValue / ((payload.Length + addBytes + 8) * POW)",
                                   "8___________C______F_E______________F___A______________F_9________F______D__F",
                                   "0____________________________________________________________________________");
            PrintColor.printColorL("Short Form: X = A / ((B + C + D) * E)",
                                   "8___________C_F_E_F_F_A_F_9_F_F____DF",
                                   "0____________________________________");
            Console.WriteLine("X: result of the formula.\r\n   The result of our POW function must be smaller than this, not the nonce.");
            Console.WriteLine("A: This is a constant: {0}",ulong.MaxValue);
            Console.WriteLine("B: This is the length of the message");
            Console.WriteLine("C: This is the additional payload bytes. In bitmessage it is 14000,\r\n   but here you can change it.");
            Console.WriteLine("D: This is 8 (the size in bytes of A) and is fixed");
            Console.WriteLine("E: This is the POW requirement for the address. 1.0 equals 320");
            Console.WriteLine();
            Console.WriteLine("Enter values below. Enter nothing to use defaults");
            Console.Write("Enter the value for B: ");
            try
            {
                length = int.Parse(Console.ReadLine());
            }
            catch
            {
                Console.Beep();
                Console.WriteLine("Invalid value, defaulting to 1KB of data (1000 chars) !");
                length = 1000;
            }
            Console.Write("Enter the value for C: ");
            try
            {
                pow_addbytes = int.Parse(Console.ReadLine());
            }
            catch
            {
                Console.Beep();
                Console.WriteLine("Invalid value, defaulting to 14000!");
                pow_addbytes = 14000;
            }
            Console.Write("Enter the value for E: ");
            try
            {
                pow = int.Parse(Console.ReadLine());
            }
            catch
            {
                Console.Beep();
                Console.WriteLine("Invalid value, defaulting to 320!");
                pow = 320;
            }


            Console.WriteLine(@"A is {0,20}.
X is {1,20}. Our result (not nonce!) needs to be smaller than this.",ulong.MaxValue,getTarget((ulong)length, (ulong)pow_addbytes, (ulong)pow));
            Console.ReadKey(true);
            Console.Clear();
            Console.WriteLine("Calculating...");
            Console.WriteLine("{0,20} {1,20} Seconds","nonce","result");
            nonceFound+=new NonceFoundHandler(Program_nonceFound);

            //Usually we would only do this once but since we
            //are benchmarking here, we keep this running until
            //the user presses a key to abort.
            while (!Console.KeyAvailable)
            {
                //Random data, but could be real message
                byte[] buffer = new byte[length];
                new Random().NextBytes(buffer);

                //in case the user selects very easy POW
                //requirement, we make the list still readable with this.
                Thread.Sleep(500);
                calcNonce(buffer,pow_addbytes,pow);
                while (cont && !Console.KeyAvailable)
                {
                    Thread.Sleep(10);
                }
            }

        }

        /// <summary>
        /// Finds a result for a payload
        /// </summary>
        /// <param name="PayLoadLength">Length of data</param>
        /// <param name="addBytes">additional bytes</param>
        /// <param name="POW">minimum POW</param>
        /// <returns>Result of a fancy formula</returns>
        private static ulong getTarget(ulong PayLoadLength, ulong addBytes, ulong POW)
        {
            return ulong.MaxValue / (ulong)((PayLoadLength + addBytes + 8) * POW);
        }

        /// <summary>
        /// Calculates a nonce
        /// </summary>
        /// <param name="payload">Data (message)</param>
        /// <param name="addBytes">additional bytes</param>
        /// <param name="POW">minimum POW</param>
        private static void calcNonce(byte[] payload, int addBytes, int POW)
        {
            var Hasher = SHA512.Create();
            var hash = Hasher.ComputeHash(payload);
            ulong target = getTarget((ulong)payload.Length, (ulong)addBytes, (ulong)POW);
            Hasher.Clear();

            T = new Thread[Environment.ProcessorCount];
            nonce = new ulong[Environment.ProcessorCount];
            object[] arg = new object[]
            {
                0,
                Environment.ProcessorCount,
                hash,
                target
            };

            cont = true;
            DT = DateTime.Now;

            for (int i = 0; i < T.Length; i++)
            {
                arg[0] = i;
                T[i] = new Thread(new ParameterizedThreadStart(doCalc));
                T[i].IsBackground = true;
                T[i].Priority = ThreadPriority.BelowNormal;
                T[i].Start(arg.Clone());
            }
        }

        /// <summary>
        /// Executed, when a nonce was found
        /// </summary>
        /// <param name="nonce">nonce</param>
        /// <param name="result">found result (not needed in productive environment)</param>
        private static void Program_nonceFound(ulong nonce,ulong result)
        {
            Console.WriteLine("{0,20} {1,20} {2,7:0.000}", nonce, result, DateTime.Now.Subtract(DT).TotalSeconds);
            cont = false;
        }

        /// <summary>
        /// Thread to calculate a nonce
        /// </summary>
        /// <param name="arg">Argument collection. See comment inside function</param>
        private static void doCalc(object arg)
        {
            //args format:
            //0: (int)     Index of thread
            //1: (int)     Number of threads
            //2: (byte[])  hash
            //3: (ulong)   Target nonce
            
            object[] args = (object[])arg;

            int id = (int)args[0];
            int inc = (int)args[1];
            byte[] hash = (byte[])args[2];
            ulong target = (ulong)args[3];
            ulong result = ulong.MaxValue;
            nonce[id] = ulong.MaxValue / (ulong)inc * (ulong)id;

            var SHA1 = SHA512.Create();
            var SHA2 = SHA512.Create();

            //We loop until the result is small enough.
            //cont=false is to cancel all threads
            while (result>target && cont)
            {
                //This stupid var_int is actually quite ineffective
                byte[] var_int = VarInt.getByte(nonce[id]);
                //This array holds the var_int and the hash
                byte[] dest = new byte[var_int.Length + hash.Length];

                //copy var_int and hash to destination
                var_int.CopyTo(dest, 0);
                hash.CopyTo(dest, var_int.Length);

                //Double round of SHA256 and converting the result to ULONG
                result=BitConverter.ToUInt64(SHA2.ComputeHash(SHA1.ComputeHash(dest)), 0);
                //next nonce
                ++nonce[id];
            }
            if (cont)
            {
                //The -1 is actually important.
                nonceFound(--nonce[id],result);
            }
        }
    }
}
