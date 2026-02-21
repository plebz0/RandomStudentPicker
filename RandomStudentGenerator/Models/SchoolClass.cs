using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RandomStudentGenerator.Models
{
    public class SchoolClass : INotifyPropertyChanged
    {
        private string _className;
        private int[] _lastAskedStudents;

        public string ClassName 
        { 
            get => _className;
            set 
            { 
                if (_className != value)
                {
                    _className = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Student> Students { get; }

        public int[] LastAskedStudents 
        { 
            get => _lastAskedStudents;
            private set
            {
                if (_lastAskedStudents != value)
                {
                    _lastAskedStudents = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SchoolClass(string className)
        {
            _className = className ?? string.Empty;
            Students = new ObservableCollection<Student>();
            _lastAskedStudents = new int[3] { -1, -1, -1 }; 
        }

        public void UpdateLastAskedStudent(int studentIndex)
        {
            studentIndex += 1;
            _lastAskedStudents[2] = _lastAskedStudents[1];
            _lastAskedStudents[1] = _lastAskedStudents[0];
            _lastAskedStudents[0] = studentIndex;
            
            OnPropertyChanged(nameof(LastAskedStudents));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString() => ClassName;
    }
}
