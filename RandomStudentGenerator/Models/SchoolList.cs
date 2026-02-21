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

        public void DeleteStudent(Student student, SchoolList schoolList)
        {
            foreach (var schoolClass in schoolList.Classes)
            {
                if (schoolClass.Students.Contains(student))
                {
                    int index = schoolClass.Students.IndexOf(student); 

                    for (int i = 0; i < 3; i++)
                    {
                        if (schoolClass.LastAskedStudents[i] > index)
                            schoolClass.LastAskedStudents[i]--; 
                        else if (schoolClass.LastAskedStudents[i] == index)
                            schoolClass.LastAskedStudents[i] = -1; 
                    }

                    schoolClass.Students.RemoveAt(index);
                    
                    for (int i = index; i < schoolClass.Students.Count; i++)
                    {
                        schoolClass.Students[i].Id = i + 1;
                    }

                    break;
                }
            }
        }

        public async Task StartLoadFromFile() { 
            var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Wybierz note.txt" });
            if (result == null) return;

            var temp = Path.Combine(FileSystem.CacheDirectory, result.FileName);
            using (var stream = await result.OpenReadAsync())
            using (var fs = File.Create(temp))
                await stream.CopyToAsync(fs);

            this.LoadFromFile(temp);
        } 

        public String StartSaveToFile() {
            var path = Path.Combine(FileSystem.AppDataDirectory, "note.txt");
            this.SaveToFile(path);
            Clipboard.SetTextAsync(path);
            return path;
        }

        public List<Student> GetAviableStudents(int classIndex)
        {
            if (classIndex < 0 || classIndex >= Classes.Count) return new List<Student>();

            var schoolClass = Classes[classIndex];
            var availableStudents = schoolClass.Students
                .Where(s => s.IsPresent && !schoolClass.LastAskedStudents.Contains(s.Id) && s.Id != LuckyNumber)
                .ToList();

            return availableStudents;
        }

        public async Task<Student?> DrawRandomStudent( int classIndex ){            
            
            List<Student> AvailableStudents = GetAviableStudents(classIndex);

            if(AvailableStudents.Count > 0)
            {

                Random rnd = new Random();

                //if(AvailableStudents.Count <= 0)
                //{
                //    drawButton.Text = "Brak uczniów do wylosowania";
                //    return;
                //}
                int draw = rnd.Next(0, AvailableStudents.Count);

                //Drugi sposób losowania z efektami wizualnymi
                // int[] delays = { 100, 150, 200, 250, 300, 350, 400, 500, 600, 700, 800, 900, 1000 };

                // for (int i = 0; i < delays.Length; i++)
                // {
                //     draw = rnd.Next(0, AvailableStudents.Count);

                //     AvailableStudents[draw].Highlighted = true;

                //     await Task.Delay(delays[i]); 

                //     AvailableStudents[draw].Highlighted = false;
                // }

                int draws = rnd.Next(AvailableStudents.Count/2, AvailableStudents.Count*2);
                for(int i = 0; i < draws; i++)
                {
                    draw = (draw+1)%AvailableStudents.Count;
                    AvailableStudents[draw].Highlighted = true;

                    await Task.Delay(100 + (int)(Math.Pow(2, 10 * i / draws))); 

                    AvailableStudents[draw].Highlighted = false;
                }

                if (classIndex != -1)
                {
                    var drawnStudent = AvailableStudents[draw];

                    var schoolClass = this.Classes[classIndex];
                    int drawnStudentIndex = schoolClass.Students.IndexOf(drawnStudent);
                    
                    this.Classes[classIndex].UpdateLastAskedStudent(drawnStudentIndex);
                    
                    return drawnStudent;
                }
                return null;
            }
            this.Classes[classIndex].UpdateLastAskedStudent(-1);
            return null;
        }

        public void AddClassToList(String name, SchoolList schoolList)
        {
            var schoolClass = new SchoolClass(name);
            schoolList.Classes.Add(schoolClass);
            
        }
        public void AddStudentToClass(String name, int selectedIndex, SchoolList schoolList) {
            if (!string.IsNullOrEmpty(name) && selectedIndex != -1)
            {
                var schoolClass = schoolList.Classes[selectedIndex];
                var student = new Student(name);
                student.Id = schoolClass.Students.Count + 1;
                schoolClass.Students.Add(student);
            }
        }

        public void RollLuckyNumber(SchoolList schoolList, int maxStudents)
        {
            Random rnd = new Random();
            schoolList.LuckyNumber = rnd.Next(1, maxStudents + 1);

        }
    }

}
