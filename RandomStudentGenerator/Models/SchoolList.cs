using System;
using System.Collections.ObjectModel;


namespace RandomStudentGenerator.Models
{
    public class SchoolList
    {
        public ObservableCollection<SchoolClass> Classes { get; }
        public int LuckyNumber { get; set; }
        public int MaxStudentsPerClass { get; private set; }

        public SchoolList()
        {
            Classes = new ObservableCollection<SchoolClass>();
            LuckyNumber = 0;
            RecalculateMaxStudents();
        }

        public void RecalculateMaxStudents()
        {
            int maxStudents = 0;
            foreach (var cls in Classes)
            {
                if (cls.Students.Count > maxStudents) maxStudents = cls.Students.Count;
                MaxStudentsPerClass = maxStudents;
            }
        }
        public void SaveToFile(string path)
        {
            var lines = new List<string> { LuckyNumber.ToString(), string.Empty };

            foreach (var cls in Classes)
            {
                lines.Add(cls.ClassName);
                for (int i = 0; i < 3; i++)
                {
                    lines.Add((cls.LastAskedStudents?.ElementAtOrDefault(i) ?? 0).ToString());
                }
                foreach (var s in cls.Students)
                {
                    lines.Add(s.IsPresent ? "O" : "N");
                    lines.Add(s.Name ?? string.Empty);
                }
                lines.Add(string.Empty);
            }
            File.WriteAllLines(path, lines);
        }

        public void Clear()
        {
            Classes.Clear();
            LuckyNumber = 0;
            MaxStudentsPerClass = 0;
        }

        public void LoadFromFile(string path)
        {
            if (!File.Exists(path)) return;
            Clear();

            var lines = File.ReadAllLines(path);
            int i = 0;


            while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i])) i++;

            // spróbuj sparsować szczęśliwy numer 
            if (i < lines.Length && int.TryParse(lines[i].Trim(), out var lucky))
            {
                LuckyNumber = lucky;
                i++;
            }

            while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i])) i++;

            while (i < lines.Length)
            {
              
                if (string.IsNullOrWhiteSpace(lines[i])) { i++; continue; }

                var className = lines[i++].Trim();
                var cls = new SchoolClass(className);

                for (int j = 0; j < 3 && i < lines.Length; j++, i++)
                {
                    if (int.TryParse(lines[i]?.Trim(), out var num)) cls.LastAskedStudents[j] = num;
                }
                    

                int studentId = 1;
                while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
                {
                    string status = lines[i++].Trim(); 
                    if (i >= lines.Length) break;  
                    string name = lines[i++].Trim();
                    var isPresent = status.Equals("O", StringComparison.OrdinalIgnoreCase);
                    var student = new Student(name, isPresent);
                    student.Id = studentId++;
                    cls.Students.Add(student);
                }

                while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i])) i++;

                Classes.Add(cls);
            }

            RecalculateMaxStudents();
        }


    }

}
