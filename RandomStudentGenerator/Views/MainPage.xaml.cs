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
            try {
                await schoolList.StartLoadFromFile();

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
                var path = schoolList.StartSaveToFile();
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
                schoolList.AddClassToList(name, schoolList);
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
            schoolList.AddStudentToClass(name, selectedIndex, schoolList);
            

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

            schoolList.DeleteStudent(student, schoolList);
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
            if (AvailableStudents.Count <= 0) drawButton.Text = "Brak uczniów do wylosowania";
            else drawButton.Text = "Losuj ucznia";
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
            var stud = await schoolList.DrawRandomStudent( schoolClassPickerToDraw.SelectedIndex );
            

            if (stud == null)
            {
                await DisplayAlert("Brak uczniów", "Nie można wylosować ucznia. Sprawdź, czy są dostępni uczniowie do losowania.", "OK");
                labelDrawnStudent.Text = "----- -----";
            }
            else
            {
                labelDrawnStudent.Text = stud.Name;
            }

            UpdateAvailableStudents();

        }

        private void OnRollLuckyNumber(object sender, EventArgs e)
        {
            int maxStudents = schoolList.MaxStudentsPerClass;

            if (maxStudents <= 0)
            {
                
                DisplayAlert("Błąd", "Brak uczniów w klasach", "OK");
                return;
            }
            
            schoolList.RollLuckyNumber(schoolList, maxStudents);
            labelLuckyNumber.Text = schoolList.LuckyNumber.ToString();
            UpdateAvailableStudents();
        }
    }
}
