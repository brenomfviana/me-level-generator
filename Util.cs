using System;

namespace LevelGenerator
{
    public static class Util
    {
        public static Random rnd = new Random();
        
        public enum Direction
        {
            right = 0,
            down = 1,
            left = 2
        };

        private static int ID = 0;

        public static int getNextId()
        {
            return ID++;
        }

        public static bool IsValidUri(string uri)
        {
            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                return false;
            Uri tmp;
            if (!Uri.TryCreate(uri, UriKind.Absolute, out tmp))
                return false;
            return tmp.Scheme == Uri.UriSchemeHttp || tmp.Scheme == Uri.UriSchemeHttps;
        }

        public static bool OpenUri(string uri)
        {
            if (!IsValidUri(uri))
                return false;
            System.Diagnostics.Process.Start(uri);
            return true;
        }
    }

    public class Parameters
    {
        // Fitness parameters
        public int nV = 20, nK = 4, nL = 4;          // #
        public float lCoef = 1.5f;                   // #
        
        public Parameters(
            int _nV,
            int _nK,
            int _nL,
            float _lCoef
        ) {
            nV = _nV;
            nK = _nK;
            nL = _nL;
            lCoef = _lCoef;
        }
    }
}