using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCL_ODA_TestPAD.Utility
{
    /// <summary>
    /// Provides random numbers of all data types in specified ranges.
    /// It also contains a couple of methods from Normally (Gaussian) distributed
    /// random numbers and Exponentially distributed random numbers.
    /// </summary>
    public class RandomGen
    {
        private static Random _RNG1;
        private static double _StoredUniformDeviate;
        private static bool _StoredUniformDeviateIsGood = false;

        #region -- Construction/Initialization --

        static RandomGen()
        {
            Reset();
        }
        public static void Reset()
        {
            _RNG1 = new Random(Environment.TickCount);
        }

        #endregion

        #region -- Uniform Deviates --

        /// <summary>
        /// Returns double in the range [0, 1)
        /// </summary>
        public static double Next()
        {
            return _RNG1.NextDouble();
        }

        /// <summary>
        /// Returns true or false randomly.
        /// </summary>
        public static bool NextBoolean()
        {
            if (_RNG1.Next(0, 2) == 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Returns double in the range [0, 1)
        /// </summary>
        public static double NextDouble()
        {
            double rn = _RNG1.NextDouble();
            return rn;
        }

        /// <summary>
        /// Returns Int16 in the range [min, max)
        /// </summary>
        public static Int16 Next(Int16 min, Int16 max)
        {
            if (max <= min)
            {
                string message = "Max must be greater than min.";
                throw new ArgumentException(message);
            }
            double rn = (max * 1.0 - min * 1.0) * _RNG1.NextDouble() + min * 1.0;
            return Convert.ToInt16(rn);
        }

        /// <summary>
        /// Returns Int32 in the range [min, max)
        /// </summary>
        public static int Next(int min, int max)
        {
            return _RNG1.Next(min, max);
        }

        /// <summary>
        /// Returns Int64 in the range [min, max)
        /// </summary>
        public static Int64 Next(Int64 min, Int64 max)
        {
            if (max <= min)
            {
                string message = "Max must be greater than min.";
                throw new ArgumentException(message);
            }

            double rn = (max * 1.0 - min * 1.0) * _RNG1.NextDouble() + min * 1.0;
            return Convert.ToInt64(rn);
        }

        /// <summary>
        /// Returns float (Single) in the range [min, max)
        /// </summary>
        public static Single Next(Single min, Single max)
        {
            if (max <= min)
            {
                string message = "Max must be greater than min.";
                throw new ArgumentException(message);
            }

            double rn = (max * 1.0 - min * 1.0) * _RNG1.NextDouble() + min * 1.0;
            return Convert.ToSingle(rn);
        }

        /// <summary>
        /// Returns double in the range [min, max)
        /// </summary>
        public static double Next(double min, double max)
        {
            if (max <= min)
            {
                string message = "Max must be greater than min.";
                throw new ArgumentException(message);
            }

            double rn = (max - min) * _RNG1.NextDouble() + min;
            return rn;
        }

        /// <summary>
        /// Returns DateTime in the range [min, max)
        /// </summary>
        public static DateTime Next(DateTime min, DateTime max)
        {
            if (max <= min)
            {
                string message = "Max must be greater than min.";
                throw new ArgumentException(message);
            }
            long minTicks = min.Ticks;
            long maxTicks = max.Ticks;
            double rn = (Convert.ToDouble(maxTicks)
               - Convert.ToDouble(minTicks)) * _RNG1.NextDouble()
               + Convert.ToDouble(minTicks);
            return new DateTime(Convert.ToInt64(rn));
        }

        /// <summary>
        /// Returns TimeSpan in the range [min, max)
        /// </summary>
        public static TimeSpan Next(TimeSpan min, TimeSpan max)
        {
            if (max <= min)
            {
                string message = "Max must be greater than min.";
                throw new ArgumentException(message);
            }

            long minTicks = min.Ticks;
            long maxTicks = max.Ticks;
            double rn = (Convert.ToDouble(maxTicks)
               - Convert.ToDouble(minTicks)) * _RNG1.NextDouble()
               + Convert.ToDouble(minTicks);
            return new TimeSpan(Convert.ToInt64(rn));
        }

        /// <summary>
        /// Returns double in the range [min, max)
        /// </summary>
        public static double NextUniform()
        {
            return Next();
        }

        /// <summary>
        /// Returns a uniformly random integer representing one of the values 
        /// in the enum.
        /// </summary>
        public static int NextEnum(Type enumType)
        {
            int[] values = (int[])Enum.GetValues(enumType);
            int randomIndex = Next(0, values.Length);
            return values[randomIndex];
        }

        #endregion

        #region -- Exponential Deviates --

        /// <summary>
        /// Returns an exponentially distributed, positive, random deviate 
        /// of unit mean.
        /// </summary>
        public static double NextExponential()
        {
            double dum = 0.0;
            while (dum == 0.0)
                dum = NextUniform();
            return -1.0 * System.Math.Log(dum, System.Math.E);
        }

        #endregion

        #region -- Normal Deviates --

        /// <summary>
        /// Returns a normally distributed deviate with zero mean and unit 
        /// variance.
        /// </summary>
        public static double NextNormal()
        {
            // based on algorithm from Numerical Recipes
            if (_StoredUniformDeviateIsGood)
            {
                _StoredUniformDeviateIsGood = false;
                return _StoredUniformDeviate;
            }
            else
            {
                double rsq = 0.0;
                double v1 = 0.0, v2 = 0.0, fac = 0.0;
                while (rsq >= 1.0 || rsq == 0.0)
                {
                    v1 = 2.0 * Next() - 1.0;
                    v2 = 2.0 * Next() - 1.0;
                    rsq = v1 * v1 + v2 * v2;
                }
                fac = System.Math.Sqrt(-2.0
                   * System.Math.Log(rsq, System.Math.E) / rsq);
                _StoredUniformDeviate = v1 * fac;
                _StoredUniformDeviateIsGood = true;
                return v2 * fac;
            }
        }

        #endregion

        public static void Test()
        {
            // Random double [0,1)
            Console.WriteLine("Random Double [0,1) = " + RandomGen.Next());

            // Random Int64 [50,100)
            long ran = RandomGen.Next(50, 100);
            Console.WriteLine("Random Int64 [50,100) = " + ran);

            // Random Boolean
            Console.WriteLine("Random Boolean = " + RandomGen.NextBoolean());

            // Random DateTime [1/1/1998, 6/1/1998)
            DateTime minDT = DateTime.Parse("1/1/1998");
            DateTime maxDT = DateTime.Parse("6/1/1998");
            DateTime ranDT = RandomGen.Next(minDT, maxDT);
            Console.WriteLine("Random DateTime [1/1/1998, 6/1/1998) = " + ranDT);

            // Random TimeSpan [3 hours, 7 hours)
            TimeSpan minTs = new TimeSpan(3, 0, 0);
            TimeSpan maxTs = new TimeSpan(7, 0, 0);
            TimeSpan ranTs = RandomGen.Next(minTs, maxTs);
            Console.WriteLine("Random TimeSpan [3 hrs, 7 hrs) = " + ranTs);

            // Random Normal Deviate
            Console.WriteLine("Normal deviate = " + RandomGen.NextNormal());

            // Random Exponential Deviate
            Console.WriteLine("Exponential deviate = " + RandomGen.NextExponential());

        }

        //Output
        //Random Double[0, 1) = 0.147433493820687
        //Random Int64[50, 100) = 69
        //Random Boolean = False
        //Random DateTime[1 / 1 / 1998, 6 / 1 / 1998) = 2 / 4 / 1998 4:09:50 AM
        //Random TimeSpan[3 hrs, 7 hrs) = 03:58:55.0844951
        //Normal deviate = 0.277308108809515
        //Exponential deviate = 1.37058190966813
    }

}
