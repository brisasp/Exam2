using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;
using LoginRegister.Interface;
using Microsoft.Extensions.DependencyInjection;
using LoginRegister.Models;



namespace LoginRegister.View
{
    /// <summary>
    /// Lógica de interacción para AddDicatadorView.xaml
    /// </summary>
    public partial class AddDicatadorView : Window
    {
        private Random _random = new();
        private bool _habilitarColoresEspeciales = false;
        private int _verdesTotales;
        private int _verdesAcertados;
        private Stopwatch _cronometro;
        private double _mediaPuntuaciones = 100; // valor de ejemplo
        private double _ultimaPuntuacion;
        private int _fallos = 0;
        private readonly IJuegoServiceToApi _juegoService;
        private DateTime _fechaInicio;


        public AddDicatadorView()
        {
            InitializeComponent();
            _juegoService = App.Current.Services.GetService<IJuegoServiceToApi>();
            //GenerarGrid();
            IniciarPartida();
        }

        private void IniciarPartida()
        {
            _verdesTotales = 0;
            _verdesAcertados = 0;
            _fallos = 0;
            _fechaInicio = DateTime.Now;
            _cronometro = Stopwatch.StartNew();

            GameGrid.Children.Clear();

            var colores = new List<string>();

            for (int i = 0; i < 24; i++)
                colores.Add(GenerarColorAleatorio());

            colores.Add("verde"); // Forzar al menos uno
            colores = colores?.OrderBy(_ => _random.Next()).ToList() ?? new List<string>();
            //colores = colores.OrderBy(_ => _random.Next()).ToList();

            foreach (var color in colores)
            {
                if (color == "verde") _verdesTotales++;

                var boton = new Button
                {
                    Width = 70,
                    Height = 70,
                    Margin = new Thickness(5),
                    Tag = color,
                    Background = CrearFondoConImagen(color),
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand
                };

                boton.Click += OnBotonClick;
                GameGrid.Children.Add(boton);
            }

            Debug.WriteLine($"Total verdes en esta ronda: {_verdesTotales}");
        }



        private void GenerarGrid()
        {
            GameGrid.Children.Clear();

            for (int i = 0; i < 25; i++)
            {
                string color = GenerarColorAleatorio();

                var boton = new Button
                {
                    Width = 70,
                    Height = 70,
                    Margin = new Thickness(5),
                    Tag = color,
                    Background = CrearFondoConImagen(color),
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                boton.Click += OnBotonClick;

                GameGrid.Children.Add(boton);
            }
        }
        //unicamente si se supera la cifra, se añadiran los otros colores, la partida basica es verde y rojo
        private string GenerarColorAleatorio()
        {
            if (_habilitarColoresEspeciales)
            {
                int roll = _random.Next(100);
                if (roll < 50) return "verde";
                if (roll < 80) return "rojo";
                if (roll < 90) return "azul";
                return "amarillo";
            }
            else
            {
                return _random.Next(2) == 0 ? "verde" : "rojo";
            }
        }


        private Brush CrearFondoConImagen(string color)
        {
            string fileName = color switch
            {
                "verde" => "Green_Circle.png",
                "rojo" => "redCircle.jpg",
                "azul" => "blue_Circle.png",
                "amarillo" => "Yellow_Circle.png",
                _ => ""
            };

            if (string.IsNullOrEmpty(fileName))
                return Brushes.Transparent;

            string rutaCompleta = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName);

            if (!System.IO.File.Exists(rutaCompleta))
            {
                MessageBox.Show($"No se encontró la imagen: {rutaCompleta}");
                return Brushes.Transparent;
            }

            return new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(rutaCompleta)),
                Stretch = Stretch.UniformToFill
            };
        }


        private async void OnBotonClick(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            if (boton == null) return;

            string color = (string)boton.Tag;

            switch (color)
            {
                case "verde":
                    _verdesAcertados++;
                    break;
                case "rojo":
                    // Penalización opcional
                    _fallos++;

                    break;
                case "azul":
                    CongelarJuego(2000);
                    break;
                case "amarillo":
                    _ultimaPuntuacion *= 2;
                    break;
            }
            boton.IsEnabled = false;
            boton.Opacity = 0.4;
            bool quedanVerdes = GameGrid.Children
                .OfType<Button>()
                .Any(b => b.IsEnabled && (string)b.Tag == "verde");

            if (!quedanVerdes)
            {
                await FinalizarPartida();
            }
        }

        private async void CongelarJuego(int milisegundos)
        {
            GameGrid.IsEnabled = false;
            await Task.Delay(milisegundos);
            GameGrid.IsEnabled = true;
        }

        private async Task FinalizarPartida()

        {
            _cronometro.Stop();

            double segundos = _cronometro.Elapsed.TotalSeconds;
            double velocidad = 1000 / segundos;

            int aciertos = _verdesAcertados;
            int fallos = _fallos;


            _ultimaPuntuacion = velocidad + (aciertos * 10) - (fallos * 5);

            if (_ultimaPuntuacion > _mediaPuntuaciones)
            {
                _habilitarColoresEspeciales = true;
            }

            MessageBox.Show($"Puntuación: {_ultimaPuntuacion:F0}\nTiempo: {segundos:F2} segundos", "Resultado");

            await EnviarResultadoAsync();
            IniciarPartida();
        }
        private async Task EnviarResultadoAsync()
        {
            try
            {
                var userName = App.Current.Properties["userName"]?.ToString();

                var dto = new DicatadorDTO
                {
                    Name = userName,
                    Usuario = 0,
                    Resultado = (int)Math.Round(_ultimaPuntuacion),
                    FechaInicio = _fechaInicio,
                    FechaFin = DateTime.Now
                };
               

                await _juegoService.PostDicatador(dto);
                MessageBox.Show("✅ Resultado enviado correctamente a la API.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar resultado: {ex.Message}");
            }
        }

    }
}
