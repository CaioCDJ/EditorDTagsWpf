using System;
using System.Drawing;
using TagLib;
using System.IO;
using NAudio;
using NAudio.Wave;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Threading;
using System.Xaml;
using System.Windows.Forms.VisualStyles;

namespace EditorTagMp3
{
    public partial class MainWindow : Window
    {
        Picture pic = new Picture();
        WaveOut wave = new WaveOut();
        OpenFileDialog arq = new OpenFileDialog();
        string rota,rotaImg;
        bool tocando;
        int larg, altu;

        public MainWindow()
        {
            InitializeComponent();
           
        }
        /*
         * Selecao de arquivo mp3 para a edição 
         * Coleta e exibição de informações do arquivo mp3
         */
        private void btnProcurar_Click(object sender, RoutedEventArgs e)
        {
            arq.Filter = "MP3 files(.mp3)|*.mp3";
            if (arq.ShowDialog() == true)
            {
                tocando = false;
                var file = TagLib.File.Create(arq.FileName);
                rota = arq.FileName;
                clear();
                if (!string.IsNullOrEmpty(file.Tag.Title))
                    txtTitulo.Text = file.Tag.Title;
                if (!string.IsNullOrEmpty(file.Tag.Artists[0]))
                    txtArtista.Text = file.Tag.Artists[0];
                try
                {
                    if (!string.IsNullOrEmpty(file.Tag.Genres[0]))
                        txtGenero.Text = file.Tag.Genres[0];
                }
                catch{}
                if (!string.IsNullOrEmpty(file.Tag.Album))
                    txtAlbum.Text = file.Tag.Album;
                if (file.Tag.Year != 0 && file.Tag.Year > 0)
                    txtAno.Text = Convert.ToString(file.Tag.Year);
                
                // imagem do album
                TagLib.IPicture pic = file.Tag.Pictures[0];
                MemoryStream ms = new MemoryStream(pic.Data.Data);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                img.Source = bitmap;
                picImge.Source = img.Source;
                GradientGrid(ms);   

                grdPlayer.Visibility = Visibility.Visible;
                stpInfo.Visibility = Visibility.Visible;
            }
        }

        /*
         *  Cria um gradiente no backgroud do <frmPrincipal> 
         * utilizando as cores da capa do arquivo mp3
         */
        private void GradientGrid(MemoryStream ms)
        {
            Bitmap bmp = new Bitmap(ms);
            larg = bmp.Width;
            altu = bmp.Height;
            System.Drawing.Color[] cores = new System.Drawing.Color[2];

            cores[0]= bmp.GetPixel((larg / 2) + 40, (altu / 2)-10);
            cores[1] = bmp.GetPixel((larg / 2) - 40, (altu / 2)+10);

            // Convertendo drawing color para media color
            System.Windows.Media.Color cor1 = System.Windows.Media.Color.FromArgb(cores[0].A, cores[0].R, cores[0].G, cores[0].B);
            System.Windows.Media.Color cor2 = System.Windows.Media.Color.FromArgb(cores[1].A, cores[1].R, cores[1].G, cores[1].B);
            FrmPrincipal.Background = new LinearGradientBrush(cor2, cor1, 30);
        }

        public void clear()
        {
            txtAlbum.Clear();
            txtAno.Clear();
            txtArtista.Clear();
            txtTitulo.Clear();
            txtGenero.Clear();
        }

        private void btnPausa_Click(object sender, RoutedEventArgs e)
        {
            if (tocando == true)
                wave.Pause();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {            
            if (tocando == false)
            {
                Mp3FileReader mp3 = new Mp3FileReader(rota);
                wave.Init(mp3);
                wave.Play();
                tocando = true;
            }
            else 
                wave.Resume();
        }

        // Ajuste de volume
        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            wave.Volume = Convert.ToSingle(slider.Value);
        }

        private void btnAltImage_Click(object sender, RoutedEventArgs e)
        {
            arq.Filter = "PNG files (.png)|*.png";
            if (arq.ShowDialog() == true)
            {
                rotaImg = arq.FileName;
                btnAltImage.Content = "Imagem Selecionada";
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            var file = TagLib.File.Create(rota);
            string[] artista = new string[1];
            string[] genero = new string[1];
            artista[0] = txtArtista.Text;
            int i=0;
            genero[0] = txtGenero.Text;
            try
            {
                 i = int.Parse(txtAno.Text);
            }
            catch { }
            if(string.IsNullOrEmpty(txtArtista.Text)
                &&string.IsNullOrEmpty(txtAno.Text)&&
                string.IsNullOrEmpty(txtGenero.Text) &&
                string.IsNullOrEmpty(txtAlbum.Text) &&
                string.IsNullOrEmpty(txtTitulo.Text))
                System.Windows.Forms.MessageBox.Show("Por favor preencha uma caixa de texto para modificar a tag","Alerta",
                    System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Warning);
            if (!string.IsNullOrEmpty(txtTitulo.Text))
                file.Tag.Title = txtTitulo.Text;
            if (!string.IsNullOrEmpty(txtAno.Text) && i>0)
                file.Tag.Year = Convert.ToUInt32(txtAno.Text);
            if (!string.IsNullOrEmpty(txtAlbum.Text))
                file.Tag.Album = txtAlbum.Text;
            if(!string.IsNullOrEmpty(artista[0]))  
                file.Tag.Artists = artista;
            if (!string.IsNullOrEmpty(genero[0]))
                file.Tag.Genres = genero;
            if (!string.IsNullOrEmpty(rotaImg))
            {
                pic.Type = PictureType.FrontCover;
                pic.Description = "Cover";
                pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                pic.Data = ByteVector.FromPath(rotaImg);
                file.Tag.Pictures = new IPicture[] { pic };
            }
            if (!string.IsNullOrEmpty(artista[0]) && !string.IsNullOrEmpty(txtAlbum.Text))
                file.Tag.AlbumArtists = artista;

            file.Save();
            System.Windows.Forms.MessageBox.Show("Alteração Concluida","alerta",
                System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Warning);
        }

    }
}
