using ImageConversion.Enums;
using ImageConversion.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.Forms.MessageBox;
using Path = System.IO.Path;
using TextBox = System.Windows.Controls.TextBox;

namespace ImageConversion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string outputFolderPath;
        List<string> inputFiles = new List<string>();

        public MainWindow()
        {
            // Força o programa a usar ponto (.) como separador decimal no lugar de vírgula (,)
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            // Start-up
            InitializeComponent();
        }

        private void btnChooseInputFolder_Click(object sender, RoutedEventArgs e)
        {
            // Janela de escolher diretório de entrada
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();

                if (result != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                txtInputFolder.Text = dialog.SelectedPath;
                checkInputDirectory(dialog.SelectedPath);
            }
        }

        private void btnChooseOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            // Janela de escolher diretório de saída
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();

                if (result != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                txtOutputFolder.Text = dialog.SelectedPath;
                checkOutputDirectory(dialog.SelectedPath);
            }
        }

        private bool checkInputDirectory(string path)
        {
            // Verificação checa se o diretório de entrada existe e chama outra função para contar e listar as imagens que existem
            if (Directory.Exists(path))
            {
                inputFiles.Clear();
                inputFiles = enumerateImagesInDirectory(path);
                txtStatusBar.Text = inputFiles.Count.ToString() + " arquivo(s) encontrado(s)";
                return true;
            }
            else
            {
                showWarning("Diretório de entrada inválido!");
                return false;
            }
        }

        private bool checkOutputDirectory(string path)
        {
            // Verificação apenas checa se o diretório de saíde existe e habilita os próximos controles
            if (Directory.Exists(path))
            {
                outputFolderPath = path;
                return true;
            }
            else
            {
                showWarning("Diretório de saída inválido!");
                return false;
            }
        }

        private List<string> enumerateImagesInDirectory(string path)
        {
            // Atualmente somente verifica se existem arquivos TIF (inclui TIFF) e JPG
            // Não verifica subpastas
            List<string> tifFiles = Directory.EnumerateFiles(path, "*.tif", SearchOption.TopDirectoryOnly).ToList();
            List<string> jpgFiles = Directory.EnumerateFiles(path, "*.jpg", SearchOption.TopDirectoryOnly).ToList();

            List<string> files = new List<string>();

            files.AddRange(tifFiles);
            files.AddRange(jpgFiles);
            files.Sort();

            return files;
        }

        private void validateIfPositiveInteger(object sender, TextCompositionEventArgs e)
        {
            // Valida se o que foi digitado no campo de texto é um número inteiro positivo
            TextBox textBox = sender as TextBox;
            var newText = textBox.Text + e.Text;

            var regex = new Regex(@"^[0-9]*?$");

            if (regex.IsMatch(newText))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void cmbCropImage_DropDownClosed(object sender, EventArgs e)
        {
            var shouldCrop = (ComboBoxItem)cmbCropImage.SelectedItem;
            switch (shouldCrop.Content)
            {
                case "Não":
                    txtCropHeight.IsEnabled = false;
                    txtCropHeight.Text = "";
                    txtCropWidth.IsEnabled = false;
                    txtCropWidth.Text = "";
                    break;
                case "Sim":
                    txtCropHeight.IsEnabled = true;
                    txtCropWidth.IsEnabled = true;
                    break;
                default:
                    break;
            }
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            // Verificações de todos os campos antes de iniciar o processamento
            if (!verifyAllInputs())
            {
                return;
            }

            // Ler parâmetros de processamento
            var processingConfiguration = readProcessingConfiguration();
            if (processingConfiguration == null)
            {
                return;
            }

            // Bloquear inputs e limpar variávis
            blockInputs();
            pgrProgressBar.Value = 0;
            txtPercentageComplete.Text = "0%";

            int totalFiles = inputFiles.Count;

            // Inicia o timer
            var sw = new Stopwatch();
            sw.Restart();

            // Iniciar processamento
            for (int i = 0; i < totalFiles; i++)
            {
                txtStatusBar.Text = "Convertendo " + (i + 1) + " de " + totalFiles + " imagens...";
                await Task.Run(() => ConvertImage.StartProcessing(inputFiles[i], outputFolderPath, processingConfiguration));

                var percentageComplete = (100 * (i + 1)) / totalFiles;
                pgrProgressBar.Value = percentageComplete;
                txtPercentageComplete.Text = percentageComplete.ToString() + "%";
            }
            // Fim do processamento

            // Parar timers
            sw.Stop();

            // Atualizar status bar
            string totalTime = convertTime(sw.ElapsedMilliseconds);
            float average = (sw.ElapsedMilliseconds / 1000f) / totalFiles;
            txtStatusBar.Text = "Convertidas " + totalFiles + " imagens em " + totalTime + ". Média: " + average.ToString("0.0") + "s/imagem.";

            // Desbloquear inputs
            unblockInputs();
        }

        private ProcessingConfiguration readProcessingConfiguration()
        {
            // Função que lê as configurações de processamento

            // Ler formato para salvar imagem
            SaveFormatEnum saveFormat;
            var saveFormatSelected = (ComboBoxItem)cmbSaveFormat.SelectedItem;
            switch (saveFormatSelected.Content)
            {
                case "TIFF":
                    saveFormat = SaveFormatEnum.TIFF;
                    break;
                case "TIFF LZW":
                    saveFormat = SaveFormatEnum.TIFFLZW;
                    break;
                case "JPG 90%":
                    saveFormat = SaveFormatEnum.JPG90;
                    break;
                case "JPG 100%":
                    saveFormat = SaveFormatEnum.JPG100;
                    break;
                default:
                    return null;
            }

            // Ler se a imagem será rotacionada
            RotateFinalImageEnum rotateFinalImage;
            var rotateSelected = (ComboBoxItem)cmbRotateImage.SelectedItem;
            switch (rotateSelected.Content)
            {
                case "Não":
                    rotateFinalImage = RotateFinalImageEnum.NO;
                    break;
                case "90° CCW":
                    rotateFinalImage = RotateFinalImageEnum.R90CCW;
                    break;
                case "90° CW":
                    rotateFinalImage = RotateFinalImageEnum.R90CW;
                    break;
                case "180°":
                    rotateFinalImage = RotateFinalImageEnum.R180;
                    break;
                default:
                    return null;
            }

            // Ler se a imagem deverá ser cortada
            bool shouldCropImage;
            var shouldCrop = (ComboBoxItem)cmbCropImage.SelectedItem;
            switch (shouldCrop.Content)
            {
                case "Não":
                    shouldCropImage = false;
                    break;
                case "Sim":
                    shouldCropImage = true;
                    break;
                default:
                    return null;
            }

            // Ler os valores de corte
            int height = txtCropHeight.Text == "" ? 0 : int.Parse(txtCropHeight.Text);
            int width = txtCropWidth.Text == "" ? 0 : int.Parse(txtCropWidth.Text);

            // Salvar os valores lidos no objeto de configuração que será usado durante o processamento
            var processingConfiguration = new ProcessingConfiguration(saveFormat, rotateFinalImage, shouldCropImage, height, width);

            return processingConfiguration;
        }

        private bool verifyAllInputs()
        {
            // Verificar diretórios
            if (!checkInputDirectory(txtInputFolder.Text) || !checkOutputDirectory(txtOutputFolder.Text))
            {
                return false;
            }

            // Verificar se há arquivos para processar
            if (!inputFiles.Any())
            {
                showWarning("Não há arquivos para processar na pasta selecionada!");
                return false;
            }

            // Verificar se valores de corte da imagem são válidos
            var shouldCrop = (ComboBoxItem)cmbCropImage.SelectedItem;
            if (((string)shouldCrop.Content == "Sim") && ((txtCropHeight.Text == "") || (txtCropWidth.Text == "")))
            {
                showWarning("Tamanho de corte da imagem inválido!");
                return false;
            }

            return true;
        }

        private string convertTime(float ellapsedTime)
        {
            float seconds = ellapsedTime / 1000f;

            if (seconds < 60)
            {
                return seconds.ToString("0.00") + "s";
            }
            else
            {
                int minutes = (int)(seconds / 60);
                seconds = seconds - (minutes * 60);
                return minutes + "m" + seconds.ToString("0") + "s";
            }
        }

        private void blockInputs()
        {
            txtInputFolder.IsEnabled = false;
            btnChooseInputFolder.IsEnabled = false;
            txtOutputFolder.IsEnabled = false;
            btnChooseOutputFolder.IsEnabled = false;
            cmbSaveFormat.IsEnabled = false;
            cmbRotateImage.IsEnabled = false;
            cmbCropImage.IsEnabled = false;
            txtCropHeight.IsEnabled = false;
            txtCropWidth.IsEnabled = false;
            btnStart.IsEnabled = false;
        }

        private void unblockInputs()
        {
            txtInputFolder.IsEnabled = true;
            btnChooseInputFolder.IsEnabled = true;
            txtOutputFolder.IsEnabled = true;
            btnChooseOutputFolder.IsEnabled = true;
            cmbSaveFormat.IsEnabled = true;
            cmbRotateImage.IsEnabled = true;
            cmbCropImage.IsEnabled = true;
            btnStart.IsEnabled = true;
            if ((string)(((ComboBoxItem)cmbCropImage.SelectedItem).Content) == "Sim")
            {
                txtCropHeight.IsEnabled = true;
                txtCropWidth.IsEnabled = true;
            }
        }

        private void btnSobre_Click(object sender, RoutedEventArgs e)
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Image Conversion v" + version);
            sb.AppendLine();
            sb.AppendLine("BASE Aerofotogrametria e Projetos S.A.");
            sb.AppendLine("Henrique G. Miraldo");

            MessageBox.Show(sb.ToString(), "Sobre", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnSair_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void showWarning(string message)
        {
            MessageBox.Show(message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
