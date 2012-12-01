using System;
using System.Collections.Generic;

namespace LX.EasyDb
{
    static class Assert
    {
        public static void IsEqualTo<T>(T obj, T other)
        {
            if (!Object.Equals(obj, other))
                throw new ApplicationException(String.Format("{0} should be equals to {1}", obj, other));
        }

        public static void IsNotEqualTo<T>(T obj, T other)
        {
            if (Object.Equals(obj, other))
                throw new ApplicationException(String.Format("{0} should not be equals to {1}", obj, other));
        }

        public static void IsNull(Object obj)
        {
            if (obj != null)
                throw new ApplicationException("Expected null");
        }

        public static void IsNotNull(Object obj)
        {
            if (obj == null)
                throw new ApplicationException("Unexpected null");
        }

        public static void IsSequenceEqualTo<T>(IEnumerable<T> obj, IEnumerable<T> other)
        {
            if (obj == null)
                obj = new T[0];
            if (other == null)
                other = new T[0];
            IEnumerator<T> it1 = obj.GetEnumerator();
            IEnumerator<T> it2 = other.GetEnumerator();
            while (it1.MoveNext() && it2.MoveNext())
            {
                if (!Object.Equals(it1.Current, it2.Current))
                    throw new ApplicationException(String.Format("{0} should be equals to {1}", obj, other));
            }
            while (it1.MoveNext())
            {
                throw new ApplicationException(String.Format("{0} should be equals to {1}", obj, other));
            }
            while (it2.MoveNext())
            {
                throw new ApplicationException(String.Format("{0} should be equals to {1}", obj, other));
            }
        }
    }
}
