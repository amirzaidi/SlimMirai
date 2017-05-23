using System.Collections.Generic;

namespace Mirai
{
    struct User
    {
        internal int Rank;
        internal float Confidence;

        User(float Confidence, int Rank = 1)
        {
            this.Rank = Rank;
            this.Confidence = Confidence;
        }

        static Dictionary<ulong, User> Data = new Dictionary<ulong, User>
        {
            { 74779725393825792, new User(0.4f, 3) },   //Amir
            { 109007493279014912, new User(0.5f, 2) },  //Kaan
            { 233954595049701377, new User(0.325f) },   //Shivan
            { 87982581550702592, new User(0.275f) },    //Rens
        };

        internal static int GetRank(ulong Id)
        {
            if (Data.ContainsKey(Id))
            {
                return Data[Id].Rank;
            }

            return 1;
        }

        internal static float GetConfidence(ulong Id)
        {
            if (Data.ContainsKey(Id))
            {
                return Data[Id].Confidence;
            }

            return 0.4f;
        }
    }
}
