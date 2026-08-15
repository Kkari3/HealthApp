using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Health
{
    public class UserInputData
    {
        public double HeightCm { get; set; }
        public double WeightKg { get; set; }
        public string Gender { get; set; }
        public string Goal { get; set; }
        public DateTime DateOfBirth { get; set; }

        public string Lifestyle { get; set; }
        public string FitnessLevel { get; set; }
        public string TrainingDuration { get; set; }
        public string Gear { get; set; }
        public List<string> Preferences { get; set; }
    }
}
