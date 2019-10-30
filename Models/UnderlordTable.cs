using System.Collections.Generic;

namespace UnderlordLeagueTables.Models
{
    public class Player
    {
        public string name { get; set; }
        public int position { get; set; }
        public double point { get; set; }
        public List<ResultRound> form { get; set; }
    }

    public class ResultRound{
        public int round { get; set; }
        public double point { get; set; }
    }

    public class Result
    {
        public string name { get; set; }
        public double point { get; set; }
    }

    public class Competition
    {
        public int round { get; set; }
        public List<Result> result { get; set; }
    }

    public class UnderlordTable
    {
        public List<Player> player { get; set; }
        public List<Competition> competition { get; set; }
    }
}