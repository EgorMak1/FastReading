using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.Controls;

namespace MauiApp1.Trainings   
{
    public partial class ShulteTablePage : ContentPage
    {
        private int _currentNumber = 1;
        private DateTime _startTime;
        private readonly List<Button> _buttons = new();
        private int _errors = 0;
        private bool _timerRunning;

        public ShulteTablePage()
        {
            InitializeComponent();
            InitializeTable();
        }

        private void InitializeTable()
        {
            const int gridSize = 5;

            _currentNumber = 1;
            _errors = 0;

            ShulteTableGrid.RowDefinitions.Clear();
            ShulteTableGrid.ColumnDefinitions.Clear();
            ShulteTableGrid.Children.Clear();
            _buttons.Clear();

            for (int i = 0; i < gridSize; i++)
            {
                ShulteTableGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                ShulteTableGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            }

            var numbers = Enumerable
                .Range(1, gridSize * gridSize)
                .OrderBy(_ => Guid.NewGuid())
                .ToList();

            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    var button = new Button
                    {
                        Text = numbers[row * gridSize + col].ToString(),
                        FontSize = 20,
                        BackgroundColor = Color.FromArgb("#D3D3D3")
                    };

                    button.Clicked += OnButtonClicked;

                    _buttons.Add(button);
                    ShulteTableGrid.Add(button, col, row);
                }
            }

            NextNumberLabel.Text = $"Найди: {_currentNumber}";

            _startTime = DateTime.Now;
            _timerRunning = true;

            Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (!_timerRunning) return false;

                TimerLabel.Text = $"Время: {DateTime.Now - _startTime:hh\\:mm\\:ss}";
                return true;
            });
        }

        private async void OnButtonClicked(object? sender, EventArgs e)
        {
            if (sender is not Button button)
                return;

            int clickedNumber = int.Parse(button.Text);

            if (clickedNumber == _currentNumber)
            {
                button.BackgroundColor = Colors.Green;
                _currentNumber++;
                NextNumberLabel.Text = $"Найди: {_currentNumber}";

                Debug.WriteLine($"Текущее число: {_currentNumber}");

                if (_currentNumber > 25)
                {
                    await FinishTrainingAsync();
                    return;
                }
            }
            else
            {
                button.BackgroundColor = Colors.Red;
                _errors++;
            }

            await Task.Delay(400);
            button.BackgroundColor = Color.FromArgb("#D3D3D3");
        }

        private async Task FinishTrainingAsync()
        {
            _timerRunning = false;

            var timeSpent = DateTime.Now - _startTime;

            await DisplayAlert(
                "Тренировка завершена",
                $"Время: {timeSpent:hh\\:mm\\:ss}\nОшибки: {_errors}",
                "OK");

            await Navigation.PopAsync();
        }
        private async void OnFinishTrainingClicked(object sender, EventArgs e)
        {
            await FinishTrainingAsync();
        }
    }
}
