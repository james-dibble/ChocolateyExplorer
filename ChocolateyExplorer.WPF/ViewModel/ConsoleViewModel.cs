namespace ChocolateyExplorer.WPF.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;

    public class ConsoleViewModel : ViewModelBase
    {
        private readonly IList<string> _consoleLines;
        private string _consoleOutput;

        public ConsoleViewModel()
        {
            this._consoleLines = new List<string>();

            this._consoleOutput = string.Empty;

            this.ClearConsoleCommand = new RelayCommand(this.ClearConsole);
        }

        public ICommand ClearConsoleCommand { get; private set; }

        public string ConsoleOutput
        {
            get
            {
                if (!this._consoleLines.Any())
                {
                    return string.Empty;
                }

                return this._consoleLines.Aggregate((current, next) => current += Environment.NewLine + next);
            }
        }

        public void AddConsoleLine(string newLine, params object[] arguments)
        {
            this._consoleLines.Add(string.Format(newLine, arguments));

            this.RaisePropertyChanged(() => this.ConsoleOutput);
        }

        private void ClearConsole()
        {
            this._consoleLines.Clear();

            this.RaisePropertyChanged(() => this.ConsoleOutput);
        }
    }
}