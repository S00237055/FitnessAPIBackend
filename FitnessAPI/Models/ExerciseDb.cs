namespace FitnessAPI.Models
{
    public class ExerciseDb
    {
        public string id { get; set; }
        public string name { get; set; }
        public string category { get; set; }
        public string body_part { get; set; }
        public string equipment { get; set; }
        public Text instructions { get; set; }
        public Steps instruction_steps { get; set; }
        public string muscle_group { get; set; }
        public List<string> secondary_muscles { get; set; }
        public string target { get; set; }
        public string image { get; set; }
        public string gif_url { get; set; }
        public string created_at { get; set; }
    }

    public class Text
    {
        public string en { get; set; }
        public string tr { get; set; }
    }

    public class Steps
    {
        public List<string> en { get; set; }
        public List<string> tr { get; set; }
    }
}
