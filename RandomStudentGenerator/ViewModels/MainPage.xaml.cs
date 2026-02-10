using System;
using System.Collections.ObjectModel;
using RandomStudentGenerator.Models;


namespace RandomStudentGenerator
{
    public partial class MainPage : ContentPage
    {
        public SchoolList schoolList { get; set; }

        public ObservableCollection<Student> AvailableStudents { get; } = new ObservableCollection<Student>();
        public MainPage()
        {
            InitializeComponent();
            schoolList = new SchoolList();
            BindingContext = this;
        }
        private async void OnLoadFileClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Wybierz note.txt" });
                if (result == null) return;

                var temp = Path.Combine(FileSystem.CacheDirectory, result.FileName);
                using (var stream = await result.OpenReadAsync())
                using (var fs = File.Create(temp))
                    await stream.CopyToAsync(fs);

                schoolList.LoadFromFile(temp);

                labelLuckyNumber.Text = schoolList.LuckyNumber.ToString();

                // ustaw źródła dla pickerów
                schoolClassPicker.ItemsSource = schoolList.Classes;
                schoolClassPickerToDraw.ItemsSource = schoolList.Classes;

                // jeśli są klasy, wybieramy pierwszą i pokazujemy ją
                if (schoolList.Classes.Count > 0)
                {
                    schoolClassPickerToDraw.SelectedIndex = 0;
                    ShowSelectedClass(0);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", ex.Message, "OK");
            }
            UpdateAvailableStudents();
        }
        private void OnFileSaveClicked(object sender, EventArgs e)
        {
            try
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, "note.txt");

                schoolList.SaveToFile(path);
                Clipboard.SetTextAsync(path);
                DisplayAlert("Sukces", $"Zapisano do pliku:\n{path}", "OK");
            }
            catch (System.Exception ex)
            {
                DisplayAlert("Błąd", ex.Message, "OK");
            }
        }

        private void OnAddSchoolClass(object sender, EventArgs e)
        {
            var name = newClassName.Text?.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                var schoolClass = new SchoolClass(name);
                schoolList.Classes.Add(schoolClass);
                newClassName.Text = string.Empty;

                // odśwież pickery i wybierz nowo dodaną klasę
                schoolClassPicker.ItemsSource = schoolList.Classes;
                schoolClassPickerToDraw.ItemsSource = schoolList.Classes;
                schoolClassPickerToDraw.SelectedIndex = schoolList.Classes.Count - 1;
                ShowSelectedClass(schoolList.Classes.Count - 1);
            }
        }

        private void OnAddStudent(object sender, EventArgs e)
        {
            var name = newStudentName.Text?.Trim();
            var selectedIndex = schoolClassPicker.SelectedIndex;
            if (!string.IsNullOrEmpty(name) && selectedIndex != -1)
            {
                var schoolClass = schoolList.Classes[selectedIndex];
                var student = new Student(name);
                student.Id = schoolClass.Students.Count + 1;
                schoolClass.Students.Add(student);
            }

            newStudentName.Text = string.Empty;
            UpdateAvailableStudents();
        }

        private void ToggleAttendance(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var student = (Student)button.BindingContext;
            student.IsPresent = !student.IsPresent;
            UpdateAvailableStudents();
        }

        private void OnDeleteStudent(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var student = (Student)button.BindingContext;

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
            UpdateAvailableStudents();
        }

        private void OnClassPickerChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            var selectedIndex = picker.SelectedIndex;
            ShowSelectedClass(selectedIndex);
            UpdateAvailableStudents();
        }

        private void ShowSelectedClass(int selectedIndex)
        {
            if (schoolList?.Classes == null) return;

            if (selectedIndex >= 0 && selectedIndex < schoolList.Classes.Count)
            {
                // wyświetl tylko jedną klasę jako kolekcję jednoelementową
                selectedClassView.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<SchoolClass> { schoolList.Classes[selectedIndex] };
            }
            else
            {
                selectedClassView.ItemsSource = null;
            }
        }

        private void UpdateAvailableStudents()
        {
            AvailableStudents.Clear();
            if (schoolList?.Classes == null || schoolList.Classes.Count == 0) return;
            if (schoolClassPickerToDraw == null) return;
            var selectedIndex = schoolClassPickerToDraw.SelectedIndex;

            if (selectedIndex != -1)
            {
                foreach (var s in schoolList.Classes[selectedIndex].Students.Select((value, i) => new {i, value}))
                {

                    if (CheckIfStudentIsAvailable(s.value, s.i, schoolList.Classes[selectedIndex]))
                    {
                        AvailableStudents.Add(s.value);
                    }
                }
               
            }                 
        }
        private bool CheckIfStudentIsAvailable(Student s, int index, SchoolClass schoolClass)
        {
            // Sprawdź 3 ostatnich pytanych
            for(int i = 0; i < 3; i++)
            {
                if (index == schoolClass.LastAskedStudents[i]-1)
                {
                    return false;
                }
            }
            // Sprawdź czy jest obecny i czy ma szczęśliwy numer
            if (s.IsPresent && index != (schoolList.LuckyNumber - 1))
            {
                return true;
            }
            return false;
        }

        private async void OnDrawRandomStudent(object sender, EventArgs e)
        {
            Random rnd = new Random();
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
           
            var selectedIndex = schoolClassPickerToDraw.SelectedIndex;
            if (selectedIndex != -1 && AvailableStudents.Count > 0)
            {
                var drawnStudent = AvailableStudents[draw];
                labelDrawnStudent.Text = drawnStudent.Name;
                var schoolClass = schoolList.Classes[selectedIndex];
                int drawnStudentIndex = schoolClass.Students.IndexOf(drawnStudent);
                
                schoolClass.UpdateLastAskedStudent(drawnStudentIndex);
                
                UpdateAvailableStudents();
            }
        }

        private void OnRollLuckyNumber(object sender, EventArgs e)
        {
            Random rnd = new Random();
            int maxStudents = schoolList.MaxStudentsPerClass;
            
            if (maxStudents <= 0)
            {
                DisplayAlert("Błąd", "Brak uczniów w klasach", "OK");
                return;
            }
            
            schoolList.LuckyNumber = rnd.Next(1, maxStudents + 1);
            labelLuckyNumber.Text = schoolList.LuckyNumber.ToString();
            UpdateAvailableStudents();
        }
    }
}
