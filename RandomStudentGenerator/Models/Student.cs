using System;
using System.ComponentModel;


namespace RandomStudentGenerator.Models
{
    public class Student : INotifyPropertyChanged
    {
        private string _name;
        private bool _isPresent;
        private int _id;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public bool IsPresent
        {
            get => _isPresent;
            set
            {
                if (_isPresent != value)
                {
                    _isPresent = value;
                    OnPropertyChanged(nameof(IsPresent));
                }
            }
        }

        public bool _highlighted;

        public bool Highlighted
        {
            get => _highlighted;
            set
            {
                if (_highlighted != value)
                {
                    _highlighted = value;
                    OnPropertyChanged(nameof(Highlighted));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Student(string name, bool isPresent = true)
        {
            _name = name ?? string.Empty;
            _isPresent = isPresent;
            _id = 0;
        }
       
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString() => $"{Id} - {Name} {(IsPresent ? "(obecny)" : "(nieobecny)")}";
    }
}
