using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ionic.Zip;
using System.Drawing;
using TextBox = System.Windows.Forms.TextBox;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata;
using System.Management;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Windows.Forms.Design.AxImporter;

namespace AsistenteOnePieceWorld
{
    public partial class Principal : Form
    {
        // Declarar variables para almacenar los valores
        public bool ajustesCheckBoxValue;
        public int ajustesTrackBar1Value;
        public int ajustesTrackBar2Value;
        public string? textoLabel4;
        //Program version
        private string program_version = ""; // versi�n actual del programa
        private string program_pastebinUrl = "https://gist.github.com/Kristiansito/a481d51852be77262cb439fcbad376bf/raw"; // URL de pastebin del programa
        private string programUpdateUrl = ""; // URL de descarga de actualizaci�n
        private string newProgramVersion = "";

        //Modpack version
        private string modpack_version = ""; // versi�n actual del modpack
        private string modpack_pastebinUrl = "https://gist.github.com/Kristiansito/916f88b84378d4ce62f67a7dc7baef41/raw"; // URL de pastebin del modpack
        private string modpackUpdateUrl = ""; // URL de descarga de actualizaci�n
        private string newModpackVersion = "";

        //Forge libraries + OnePieceWorld version
        private string libraries_Url = "";

        //Distant Horizons (Optional)
        private string distantHorizons_url = "";

        private string selectedPath = ""; // Variable para almacenar la ruta seleccionada por el usuario
        private System.Windows.Forms.Timer animationTimer;
        private int animationIndex;
        private readonly string[] animationSequence = { "Descargando", "Descargando.", "Descargando..", "Descargando..." };


        public Principal()
        {
            InitializeComponent();
            CheckMinecraftIsInstalled();
            LoadCustomPath();
            // Obtener la versi�n actual del t�tulo de la ventana principal
            program_version = ObtenerVersion(this.Text);
            SaveProgramVersion();
            CheckModpackIsInstalled();
            LoadModpackVersion();
            CheckForProgramUpdates();

            InitializeProgressBar();
            animationTimer = new System.Windows.Forms.Timer();

            // FixDistant();


            // Crear el objeto ToolTip
            System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
            toolTip.AutoPopDelay = 5000; // Tiempo en milisegundos que muestra el ToolTip
            toolTip.InitialDelay = 250; // Retardo inicial antes de mostrar el ToolTip
            toolTip.ReshowDelay = 100; // Retardo antes de mostrar nuevamente el ToolTip si el cursor se mueve sobre el control

            // Asociar el ToolTip al control PictureBox5
            toolTip.SetToolTip(pictureBox5, "Restablecer ruta...");


            // Asociar el ToolTip al control PictureBox6
            toolTip.SetToolTip(pictureBox6, "Ajustes...");

            LoadUrlsFromGist().ConfigureAwait(false);
        }

        private async Task LoadUrlsFromGist()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // URL of the Gist containing the URLs
                    string gistUrl = "https://gist.github.com/Kristiansito/6072517cb19e49c7b867329549ebad09/raw";

                    // Download the gist content as a plain text
                    string gistContent = await client.GetStringAsync(gistUrl);

                    // Split the content by new lines
                    var urls = gistContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    // Ensure there are enough lines to get the URLs
                    if (urls.Length >= 2)
                    {
                        libraries_Url = urls[0];
                        distantHorizons_url = urls[1];
                    }
                    else
                    {
                        // Handle the case where not enough URLs are provided
                        libraries_Url = "default_libraries_url";
                        distantHorizons_url = "default_distantHorizons_url";
                        button1.Text = "AAAAAAAAAAAAAAAA";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading URLs from Gist:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        // Funci�n para detectar si Minecraft est� instalado
        private string CheckMinecraftIsInstalled()
        {
            // Obtiene la ruta de la carpeta .minecraft
            string minecraftPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");

            // Comprueba si la carpeta .minecraft existe
            if (!Directory.Exists(minecraftPath))
            {
                // Muestra un MessageBox de error al usuario si la carpeta no existe
                MessageBox.Show(
                    "No se ha detectado la carpeta .minecraft. Por favor, instale Minecraft o in�cielo al menos una vez.",
                    "Error: Minecraft No Encontrado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                // Cierra la aplicaci�n despu�s de que el usuario haga clic en "OK"
                Application.Exit();

                // Retorna una cadena vac�a, aunque el proceso se cerrar� antes de usarla
                return string.Empty;
            }

            // Si la carpeta existe, retorna la ruta
            return minecraftPath;
        }


        // Funci�n para extraer los n�meros de la versi�n
        private string ObtenerVersion(string titulo)
        {
            // Buscar el patr�n "(vX.Y)" en el t�tulo
            int indiceInicio = titulo.IndexOf("(v") + 2;
            int indiceFin = titulo.IndexOf(")", indiceInicio);

            if (indiceInicio >= 0 && indiceFin > indiceInicio)
            {
                // Extraer los n�meros de la versi�n entre los �ndices encontrados
                string version = titulo.Substring(indiceInicio, indiceFin - indiceInicio);
                return version;
            }

            return "";
        }

        private void InitializeProgressBar()
        {
            progressBarUI.Minimum = 0;
            progressBarUI.Maximum = 100;
            progressBarUI.Step = 1;
            progressBarUI.Visible = false;

            Controls.Add(progressBarUI);
        }

        private async void CheckForProgramUpdates()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Comprobar actualizaci�n del programa
                    string programPastebinInfo = await client.GetStringAsync(program_pastebinUrl);
                    string[] programLines = programPastebinInfo.Split('\n');
                    newProgramVersion = programLines[0].Trim();
                    programUpdateUrl = programLines[1].Trim();


                    if (program_version != newProgramVersion)
                    {
                        MessageBox.Show("Se ha encontrado una actualizaci�n del programa.\nA continuaci�n se actualizar�.", "Actualizaci�n (v" + newProgramVersion + ")", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await DownloadAndUpdateProgram();
                        label1.Text = "Modpack: v" + modpack_version;
                        return;
                    }
                    else
                    {
                        CheckForModpackUpdates();
                    }


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ha ocurrido un error al comprobar las actualizaciones:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void LoadCustomPath()
        {
            try
            {
                // Crear rutacompleta del archivo
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft", "custom_path.txt");

                // Comprobar que exista y leer ruta del archivo
                if (File.Exists(filePath))
                {
                    selectedPath = File.ReadAllText(filePath);
                    button2.Text = selectedPath;
                }
            }
            catch (FileNotFoundException)
            {
                // No hacer nada, se usar� la ruta predeterminada
            }
        }

        private void CheckModpackIsInstalled()
        {
            // Especifica la ruta del archivo instalado.txt
            string installedFilePath = Path.Combine(selectedPath, "installed.txt");

            // Comprueba si el archivo existe
            if (File.Exists(installedFilePath))
            {
                // Lee el contenido del archivo
                string content = File.ReadAllText(installedFilePath);

                // Comprueba si el contenido es "OK"
                if (content.Trim() == "OK")
                {
                    // Cambia el texto del bot�n
                    button1.Text = "REINSTALAR";
                }
            }
        }

        private void SaveCustomPath()
        {
            // Crear rutacompleta del archivo
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft", "custom_path.txt");

            // Escribir ruta seleccionada en el archivo
            File.WriteAllText(filePath, selectedPath);
        }

        private async void LoadModpackVersion()
        {
            // Cargar versi�n del modpack
            string installedFilePath = Path.Combine(selectedPath, "modpack_version.txt");
            if (File.Exists(installedFilePath))
            {
                modpack_version = File.ReadAllText(installedFilePath);
                label1.Visible = true;
                label1.Text = "Modpack: v" + modpack_version;
            }
            else
            {
                button1.Text = "DESCARGAR";
                using (HttpClient client = new HttpClient())
                {
                    // Comprobar actualizaci�n del modpack
                    string modpackPastebinInfo = await client.GetStringAsync(modpack_pastebinUrl);
                    string[] modpackLines = modpackPastebinInfo.Split('\n');
                    newModpackVersion = modpackLines[0].Trim();
                    modpackUpdateUrl = modpackLines[1].Trim();
                }
            }

        }

        private async void CheckForModpackUpdates()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Comprobar actualizaci�n del modpack
                    string modpackPastebinInfo = await client.GetStringAsync(modpack_pastebinUrl);
                    string[] modpackLines = modpackPastebinInfo.Split('\n');
                    newModpackVersion = modpackLines[0].Trim();
                    modpackUpdateUrl = modpackLines[1].Trim();

                    if (button1.Text != "DESCARGAR")
                    {
                        if (modpack_version != newModpackVersion)
                        {
                            button1.Text = "ACTUALIZAR";
                            MessageBox.Show("Se ha encontrado una actualizaci�n del pack de mods", "Actualizaci�n (v" + newModpackVersion + ")", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            button1.Text = "REINSTALAR";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ha ocurrido un error al comprobar las actualizaciones:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /*
        private void FixDistant()
        {
            // Nombre del directorio a buscar
            string oldDirectoryName = "play%2Eluminakraft%2Ecom25983";
            string oldDirectoryPath = Path.Combine(selectedPath, oldDirectoryName);

            // Nombre del nuevo directorio
            string newDirectoryName = "Distant_Horizons_server_data";
            string newDirectoryPath = Path.Combine(selectedPath, newDirectoryName);

            // Verifica si el directorio viejo existe
            if (Directory.Exists(oldDirectoryPath))
            {
                // Si el directorio nuevo ya existe, eliminarlo antes de mover
                if (Directory.Exists(newDirectoryPath))
                {
                    Directory.Delete(newDirectoryPath, true); // Elimina el directorio y su contenido
                    Console.WriteLine($"Directorio eliminado: {newDirectoryPath}");
                }

                // Crea el directorio nuevo
                Directory.CreateDirectory(newDirectoryPath);
                Console.WriteLine($"Directorio creado: {newDirectoryPath}");

                // Mueve el directorio viejo al nuevo directorio
                string newOldDirectoryPath = Path.Combine(newDirectoryPath, oldDirectoryName);
                Directory.Move(oldDirectoryPath, newOldDirectoryPath);
                Console.WriteLine($"Directorio movido a: {newOldDirectoryPath}");
            }
            else
            {
                Console.WriteLine($"El directorio viejo no existe: {oldDirectoryPath}");
            }
        }
        */



        private async Task DownloadAndUpdateProgram()
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            pictureBox1.Enabled = false;
            pictureBox5.Enabled = false;
            pictureBox6.Enabled = false;
            this.ControlBox = false;
            Cursor = Cursors.WaitCursor;
            // Iniciar animacion Descargando...
            button1.Text = animationSequence[animationIndex];
            animationTimer.Interval = 500;
            animationTimer.Tick += (s, e) =>
            {
                animationIndex = (animationIndex + 1) % animationSequence.Length;
                button1.Text = animationSequence[animationIndex];
            };

            animationTimer.Start();
            try
            {
                // Descargar nuevo programa
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(programUpdateUrl);
                    response.EnsureSuccessStatusCode();
                    byte[] programBytes = await response.Content.ReadAsByteArrayAsync();

                    // Obtener la ruta de la carpeta de Descargas del usuario
                    string downloadsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";

                    // Crear el nombre del nuevo archivo
                    string newProgramFileName = $"Asistente de OnePieceWorld v{newProgramVersion}.exe";

                    // Guardar el nuevo programa en la carpeta de Descargas
                    string newProgramFilePath = Path.Combine(downloadsFolderPath, newProgramFileName);
                    File.WriteAllBytes(newProgramFilePath, programBytes);

                    // Mostrar mensaje de confirmaci�n
                    MessageBox.Show("La actualizaci�n se ha descargado correctamente. El nuevo programa se encuentra en la carpeta de Descargas.", "Descarga completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    animationTimer.Stop();

                    // Ejecutar el nuevo programa
                    Process.Start(newProgramFilePath);

                    // Cerrar la aplicaci�n actual
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ha ocurrido un error al descargar la actualizaci�n del programa:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            string url = "https://discord.gg/UJZRrcUFMj";
            System.Diagnostics.Process.Start("cmd", "/c start " + url);
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            // Cambiar el tama�o de la imagen al entrar el cursor
            pictureBox1.Size = new Size(pictureBox1.Width + 10, pictureBox1.Height + 10);
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            // Restaurar el tama�o original de la imagen al salir el cursor
            pictureBox1.Size = new Size(pictureBox1.Width - 10, pictureBox1.Height - 10);
        }


        private void button1_MouseEnter(object sender, EventArgs e)
        {
            button1.BackgroundImage = Properties.Resources.half_widgetss2;
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            button1.BackgroundImage = Properties.Resources.half_widgets2;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedPath))
            {
                // Si el usuario no ha seleccionado una ruta, usa la ruta predeterminada
                selectedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft", "_OnePieceWorld");
            }
            if (!string.IsNullOrEmpty(selectedPath))
            {
                SaveCustomPath();
            }

            progressBarUI.Visible = true;
            progressBarUI.Value = 0;
            button1.Enabled = false;
            Cursor = Cursors.WaitCursor;

            // Descargar e instalar actualizaci�n
            await DownloadFile(modpackUpdateUrl);

            progressBarUI.Visible = false;
            Cursor = Cursors.Default;
            button1.Enabled = true;
            button2.Enabled = true;
            pictureBox1.Enabled = true;
            pictureBox5.Enabled = true;
            pictureBox6.Enabled = true;
            this.ControlBox = true;

            modpack_version = newModpackVersion; // actualizar versi�n del programa
            SaveModpackVersion(); // guardar versi�n en archivo

            Environment.Exit(0);
        }

        private async Task DownloadFile(string url)
        {
            button3.Text = "DESINSTALAR";
            button1.Enabled = false;
            button3.Enabled = false;
            button2.Enabled = false;
            pictureBox1.Enabled = false;
            pictureBox5.Enabled = false;
            pictureBox6.Enabled = false;
            this.ControlBox = false;
            Cursor = Cursors.WaitCursor;

            // Iniciar animaci�n Descargando...
            button1.Text = animationSequence[animationIndex];
            animationTimer.Interval = 500;
            animationTimer.Tick += (s, e) =>
            {
                animationIndex = (animationIndex + 1) % animationSequence.Length;
                button1.Text = animationSequence[animationIndex];
            };

            animationTimer.Start();

            button2.Text = "Limpiando archivos antiguos...";
            await Task.Delay(1000);

            // Carpetas a limpiar
            string[] foldersToClean = { "mods", "config/fancymenu", };

            foreach (string folder in foldersToClean)
            {
                string folderPath = Path.Combine(selectedPath, folder);
                DirectoryInfo dir = new DirectoryInfo(folderPath);

                if (dir.Exists)
                {
                    // Eliminar archivos dentro de la carpeta
                    FileInfo[] files = dir.GetFiles();

                    foreach (FileInfo file in files)
                    {
                        file.Delete();
                    }

                    // Eliminar subcarpetas
                    DirectoryInfo[] subDirs = dir.GetDirectories();
                    foreach (DirectoryInfo subDir in subDirs)
                    {
                        DeleteDirectory(subDir);
                    }
                }
            }

            // Funci�n recursiva para eliminar una carpeta y sus contenidos
            void DeleteDirectory(DirectoryInfo directory)
            {
                // Eliminar archivos dentro de la carpeta
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }

                // Eliminar subcarpetas
                foreach (DirectoryInfo subDir in directory.GetDirectories())
                {
                    DeleteDirectory(subDir);
                }

                // Eliminar la carpeta actual
                directory.Delete();
            }

            // Crear control TextBox para el log
            TextBox logTextBox = new TextBox();
            logTextBox.Multiline = true;
            logTextBox.ScrollBars = ScrollBars.Vertical;
            logTextBox.ReadOnly = true;
            logTextBox.Width = 400;
            logTextBox.Height = 200;
            logTextBox.Location = new Point(progressBarUI.Left, progressBarUI.Top - logTextBox.Height - 10);
            logTextBox.Visible = false; // Ocultar el log inicialmente
            Controls.Add(logTextBox);
            logTextBox.BringToFront(); // Asegurar que el logTextBox est� en la capa superior

            // Crear Label para mostrar la velocidad de descarga
            Label speedLabel = new Label();
            speedLabel.AutoSize = true;
            speedLabel.ForeColor = Color.White; // Color del texto
            speedLabel.BackColor = Color.Transparent; // Sin fondo
            Controls.Add(speedLabel);

            // Crear Label para mostrar el tiempo restante
            Label timeLabel = new Label();
            timeLabel.AutoSize = true;
            timeLabel.ForeColor = Color.White; // Color del texto
            timeLabel.BackColor = Color.Transparent; // Sin fondo
            Controls.Add(timeLabel);

            // Crear carpeta _OnePieceWorld en caso de que la ruta sea por defecto
            if (selectedPath == Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft", "_OnePieceWorld"))
            {
                // Verificar si la carpeta _OnePieceWorld ya existe
                if (!Directory.Exists(selectedPath))
                {
                    // Si no existe, crear la carpeta
                    Directory.CreateDirectory(selectedPath);
                }
            }

            // Leer la decisi�n del usuario desde el archivo distant.txt
            bool downloadDistantHorizons = false;
            string distantFilePath = Path.Combine(selectedPath, "distant.txt");

            if (File.Exists(distantFilePath))
            {
                string userDecision = File.ReadAllText(distantFilePath).Trim();
                // Si distant.txt contiene "true", NO queremos descargar
                downloadDistantHorizons = !userDecision.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            // Si el archivo no existe o contiene "false", mostrar el MessageBox
            if (!File.Exists(distantFilePath) || downloadDistantHorizons)
            {
                DialogResult result = MessageBox.Show(
                    "�Desea descargar Distant Horizons?\n\n" +
                    "Esta opci�n mejorar� su experiencia permiti�ndole ver m�s lejos en el juego.\n" +
                    "Tenga en cuenta que descargar� el mapa completo, lo que puede requerir m�s de 3 GB y podr�a tardar un tiempo considerable.\n\n" +
                    "Nota: Esta descarga no es recomendable para PCs de muy bajos recursos.",
                    "Descargar Distant Horizons (recomendado)",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                downloadDistantHorizons = result == DialogResult.Yes;
            }

            DateTime startTime; // Declarar startTime aqu�
            DateTime lastUpdateTime = DateTime.Now; // Tiempo de la �ltima actualizaci�n

            button2.Text = "Descargando librer�as...";

            // Descargar archivo zip librer�as de Dropbox
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(libraries_Url, HttpCompletionOption.ResponseHeadersRead))
            using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
            {
                string fileToWriteTo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft", "librerias.zip");
                using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
                {
                    byte[] buffer = new byte[8192];
                    long totalBytes = response.Content.Headers.ContentLength ?? -1;
                    long downloadedBytes = 0;
                    startTime = DateTime.Now; // Tiempo de inicio

                    while (true)
                    {
                        int bytesRead = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            break;
                        }

                        await streamToWriteTo.WriteAsync(buffer, 0, bytesRead);
                        downloadedBytes += bytesRead;

                        if (totalBytes > 0)
                        {
                            int progress = (int)(downloadedBytes * 100 / totalBytes);
                            progressBarUI.Value = progress;

                            // Actualizar cada segundo
                            if (DateTime.Now - lastUpdateTime > TimeSpan.FromSeconds(2))
                            {
                                TimeSpan elapsedTime = DateTime.Now - startTime;
                                double bytesPerSecond = downloadedBytes / elapsedTime.TotalSeconds;
                                double bytesPerSecondInMB = bytesPerSecond / (1024 * 1024); // Convertir a MB/s
                                double estimatedTimeRemaining = (totalBytes - downloadedBytes) / bytesPerSecond;

                                // Actualizar el Label con el tiempo restante
                                timeLabel.Text = $"Tiempo restante: {TimeSpan.FromSeconds(estimatedTimeRemaining):hh\\:mm\\:ss}";

                                // Actualizar el Label con la velocidad
                                speedLabel.Text = $"{bytesPerSecondInMB:F2} MB/s";

                                // Actualizar la ubicaci�n de los Labels
                                timeLabel.Location = new Point(progressBarUI.Left - timeLabel.Width - 10, progressBarUI.Top + progressBarUI.Height / 2 - timeLabel.Height / 2 - speedLabel.Height - 5);
                                speedLabel.Location = new Point(progressBarUI.Left - speedLabel.Width - 10, progressBarUI.Top + progressBarUI.Height / 2 - speedLabel.Height / 2);

                                lastUpdateTime = DateTime.Now; // Actualizar el tiempo de la �ltima actualizaci�n
                            }

                            // Registrar el progreso en el TextBox (si est� visible)
                            if (logTextBox.Visible)
                            {
                                string message = $"Descargando: {downloadedBytes} / {totalBytes} bytes ({speedLabel.Text}, {timeLabel.Text})";
                                Invoke((MethodInvoker)(() =>
                                {
                                    logTextBox.AppendText(message + Environment.NewLine);
                                    logTextBox.ScrollToCaret();
                                }));
                            }
                        }
                    }
                }
            }

            button2.Text = "Descargando modpack (puede tardar un rato)...";

            // Descargar archivo zip modpack de Dropbox
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
            {
                string fileToWriteTo = Path.Combine(selectedPath, "modpack.zip");
                using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
                {
                    byte[] buffer = new byte[8192];
                    long totalBytes = response.Content.Headers.ContentLength ?? -1;
                    long downloadedBytes = 0;
                    startTime = DateTime.Now; // Tiempo de inicio
                    lastUpdateTime = DateTime.Now; // Tiempo de la �ltima actualizaci�n

                    while (true)
                    {
                        int bytesRead = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            break;
                        }

                        await streamToWriteTo.WriteAsync(buffer, 0, bytesRead);
                        downloadedBytes += bytesRead;

                        if (totalBytes > 0)
                        {
                            int progress = (int)(downloadedBytes * 100 / totalBytes);
                            progressBarUI.Value = progress;

                            // Actualizar cada segundo
                            if (DateTime.Now - lastUpdateTime > TimeSpan.FromSeconds(2))
                            {
                                TimeSpan elapsedTime = DateTime.Now - startTime;
                                double bytesPerSecond = downloadedBytes / elapsedTime.TotalSeconds;
                                double bytesPerSecondInMB = bytesPerSecond / (1024 * 1024); // Convertir a MB/s
                                double estimatedTimeRemaining = (totalBytes - downloadedBytes) / bytesPerSecond;

                                // Actualizar el Label con el tiempo restante
                                timeLabel.Text = $"Tiempo restante: {TimeSpan.FromSeconds(estimatedTimeRemaining):hh\\:mm\\:ss}";

                                // Actualizar el Label con la velocidad
                                speedLabel.Text = $"{bytesPerSecondInMB:F2} MB/s";

                                // Actualizar la ubicaci�n de los Labels
                                timeLabel.Location = new Point(progressBarUI.Left - timeLabel.Width - 10, progressBarUI.Top + progressBarUI.Height / 2 - timeLabel.Height / 2 - speedLabel.Height - 5);
                                speedLabel.Location = new Point(progressBarUI.Left - speedLabel.Width - 10, progressBarUI.Top + progressBarUI.Height / 2 - speedLabel.Height / 2);

                                lastUpdateTime = DateTime.Now; // Actualizar el tiempo de la �ltima actualizaci�n
                            }

                            // Registrar el progreso en el TextBox (si est� visible)
                            if (logTextBox.Visible)
                            {
                                string message = $"Descargando: {downloadedBytes} / {totalBytes} bytes ({speedLabel.Text}, {timeLabel.Text})";
                                Invoke((MethodInvoker)(() =>
                                {
                                    logTextBox.AppendText(message + Environment.NewLine);
                                    logTextBox.ScrollToCaret();
                                }));
                            }
                        }
                    }
                }
            }

            // Desactivar la animaci�n de "Descargando..."
            animationTimer.Stop();

            button1.Text = "Un segundo...";
            button2.Text = "Extrayendo librer�as...";
            await Task.Delay(1500);

            // Extraer archivo librerias.zip
            using (ZipFile zip = ZipFile.Read(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft") + Path.DirectorySeparatorChar + "librerias.zip"))
            {
                zip.ExtractProgress += (sender, e) =>
                {
                    if (e.EventType == ZipProgressEventType.Extracting_AfterExtractEntry)
                    {
                        string message = $"Extrayendo: {e.CurrentEntry.FileName}";
                        Invoke((MethodInvoker)(() =>
                        {
                            logTextBox.AppendText(message + Environment.NewLine);
                            logTextBox.ScrollToCaret();
                        }));
                    }
                    else if (e.EventType == ZipProgressEventType.Extracting_EntryBytesWritten)
                    {
                        int progress = (int)(e.BytesTransferred * 100 / e.TotalBytesToTransfer);
                        Invoke((MethodInvoker)(() => progressBarUI.Value = progress));

                        // Registrar el progreso en el TextBox (si est� visible)
                        if (logTextBox.Visible)
                        {
                            string message = $"Extrayendo: {e.BytesTransferred} / {e.TotalBytesToTransfer} bytes";
                            Invoke((MethodInvoker)(() =>
                            {
                                logTextBox.AppendText(message + Environment.NewLine);
                                logTextBox.ScrollToCaret();
                            }));
                        }
                    }
                };

                await Task.Run(() => zip.ExtractAll(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft"), ExtractExistingFileAction.OverwriteSilently));
            }

            button2.Text = "Extrayendo modpack...";

            // Extraer archivo modpack.zip
            using (ZipFile zip = ZipFile.Read(selectedPath + Path.DirectorySeparatorChar + "modpack.zip"))
            {
                zip.ExtractProgress += (sender, e) =>
                {
                    if (e.EventType == ZipProgressEventType.Extracting_AfterExtractEntry)
                    {
                        string message = $"Extrayendo: {e.CurrentEntry.FileName}";
                        Invoke((MethodInvoker)(() =>
                        {
                            logTextBox.AppendText(message + Environment.NewLine);
                            logTextBox.ScrollToCaret();
                        }));
                    }
                    else if (e.EventType == ZipProgressEventType.Extracting_EntryBytesWritten)
                    {
                        int progress = (int)(e.BytesTransferred * 100 / e.TotalBytesToTransfer);
                        Invoke((MethodInvoker)(() => progressBarUI.Value = progress));

                        // Registrar el progreso en el TextBox (si est� visible)
                        if (logTextBox.Visible)
                        {
                            string message = $"Extrayendo: {e.BytesTransferred} / {e.TotalBytesToTransfer} bytes";
                            Invoke((MethodInvoker)(() =>
                            {
                                logTextBox.AppendText(message + Environment.NewLine);
                                logTextBox.ScrollToCaret();
                            }));
                        }
                    }
                };

                await Task.Run(() => zip.ExtractAll(selectedPath, ExtractExistingFileAction.OverwriteSilently));
            }


            // Guardar archivo "installed.txt"
            string installedFilePath = Path.Combine(selectedPath, "installed.txt");
            File.WriteAllText(installedFilePath, "OK");

            //Crear archivo preset.txt si no existe
            string presetPath = Path.Combine(selectedPath, "preset.txt");
            if (!File.Exists(presetPath))
            {
                File.Create(presetPath).Close();
                File.WriteAllText(presetPath, "High");
            }

            // Ruta de los directorios
            string configuredDefaultsDir = Path.Combine(selectedPath, "configureddefaults");
            string configDir = Path.Combine(selectedPath, "config");

            // Llamar a la funci�n para copiar archivos
            CopyFilesIfNotExists(configuredDefaultsDir, configDir);



            // Eliminar el archivo modpack.zip
            string fileToDelete = Path.Combine(selectedPath, "modpack.zip");
            File.Delete(fileToDelete);

            // Eliminar el archivo librerias.zip
            string fileToDelete2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft", "librerias.zip");
            File.Delete(fileToDelete2);

            // Eliminar el archivo Distant_Horizons_server_data.zip
            string fileToDelete3 = Path.Combine(selectedPath, "Distant_Horizons_server_data.zip");

            // Verificar si el archivo existe antes de intentar eliminarlo
            if (File.Exists(fileToDelete3))
            {
                try
                {
                    File.Delete(fileToDelete3);
                }
                catch (IOException)
                {
                }
            }
            else
            {
            }

            ////////////////////////////////////////
            //Crear perfil en launcher_profiles.json
            ////////////////////////////////////////

            // Determinar la memoria RAM total del sistema
            double totalMemoryGB = GetTotalMemoryInGB();

            // Determinar los argumentos de Java en funci�n de la RAM total
            string javaArgs = DetermineJavaArgs(totalMemoryGB);

            // Continuar con la creaci�n o modificaci�n del perfil
            string perfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft", "launcher_profiles.json");

            // Leer el contenido existente del archivo JSON
            var json = File.ReadAllText(perfilesPath);
            var jsonObj = JObject.Parse(json);

            // Crear el nuevo objeto JSON para "OnePieceWorld"
            var newProfile = new JObject
            {
                ["created"] = "2024-03-08T20:18:06.251Z",
                ["gameDir"] = @$"{selectedPath}",
                ["icon"] = "data:image/jpeg;base64,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",
                ["javaArgs"] = javaArgs,
                ["lastUsed"] = "2024-12-28T23:59:22.977Z",
                ["lastVersionId"] = "1.20.1-forge-47.3.12",
                ["name"] = "OnePieceWorld",
                ["type"] = "custom"
            };

            // Los perfiles est�n en una propiedad "profiles" en el JSON
            if (jsonObj["profiles"] is JObject profiles) // Usar 'is' para intentar el cast y verificar por null
            {
                profiles["OnePieceWorld"] = newProfile; // Esto agregar� el perfil "OnePieceWorld" justo debajo del �ltimo perfil existente
            }
            else
            {
                // Si "profiles" no es un JObject o es null se inicializa 'profiles' y se agrega a 'jsonObj'
                profiles = new JObject();
                profiles["OnePieceWorld"] = newProfile;
                jsonObj["profiles"] = profiles; // Aseg�rate de que 'jsonObj' contenga una propiedad "profiles"
            }

            // Escribir el JSON modificado de vuelta en el archivo
            File.WriteAllText(perfilesPath, jsonObj.ToString(Formatting.Indented));

            static double GetTotalMemoryInGB()
            {
                double totalMemory = 0;
                var query = new ObjectQuery("SELECT * FROM Win32_ComputerSystem");
                using (var searcher = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        totalMemory = Convert.ToDouble(mo["TotalPhysicalMemory"]);
                    }
                }
                return totalMemory / (1024 * 1024 * 1024); // Convertir de bytes a GB
            }

            static string DetermineJavaArgs(double totalMemoryGB)
            {
                int javaMemory = totalMemoryGB switch
                {
                    <= 8 => 4,      // Si la RAM es 8GB o menos, asigna 4
                    <= 12 => 6,     // Si la RAM es m�s de 8GB y hasta 12GB, asigna 6
                    < 23 => 10,     // Si la RAM es m�s de 12GB y menos de 24GB, asigna 10
                    _ => 12         // Para 23GB o m�s, asigna 12
                };

                return $"-Xmx{javaMemory}G -XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=32M";
            }

            ////////////////////////////////////////
            //FORZAR TEXTUREPACKS
            ////////////////////////////////////////

            // Ruta del archivo options.txt a actualizar
            string optionsFilePath = Path.Combine(selectedPath, "options.txt");

            // Ruta del archivo options.txt con las configuraciones
            string defaultOptionsFilePath = Path.Combine(selectedPath, "config", "defaultoptions", "options.txt");

            try
            {
                // Verifica si el archivo options.txt existe en selectedPath
                if (File.Exists(optionsFilePath))
                {
                    // Verifica si el archivo options.txt con configuraciones existe
                    if (File.Exists(defaultOptionsFilePath))
                    {
                        string[] defaultOptionsLines = File.ReadAllLines(defaultOptionsFilePath);
                        string[] optionsLines = File.ReadAllLines(optionsFilePath);

                        // Usa expresiones regulares para encontrar las l�neas relevantes
                        string? resourcePacksLine = null;
                        string? incompatibleResourcePacksLine = null;

                        foreach (string line in defaultOptionsLines)
                        {
                            if (line.Trim().StartsWith("resourcePacks:"))
                            {
                                resourcePacksLine = line.Trim();
                            }
                            else if (line.Trim().StartsWith("incompatibleResourcePacks:"))
                            {
                                incompatibleResourcePacksLine = line.Trim();
                            }
                        }

                        using (StreamWriter writer = new StreamWriter(optionsFilePath, false))
                        {
                            foreach (string line in optionsLines)
                            {
                                if (line.Trim().StartsWith("resourcePacks:"))
                                {
                                    writer.WriteLine(resourcePacksLine ?? "resourcePacks:[]");
                                }
                                else if (line.Trim().StartsWith("incompatibleResourcePacks:"))
                                {
                                    writer.WriteLine(incompatibleResourcePacksLine ?? "incompatibleResourcePacks:[]");
                                }
                                else
                                {
                                    writer.WriteLine(line);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Opcional: manejar excepciones de manera silenciosa o loguearlas en un archivo de registro si es necesario
            }



            if (!downloadDistantHorizons)
            {
                button2.Text = "Desactivando Distant Horizons...";

                string modsDir = Path.Combine(selectedPath, "mods");
                DirectoryInfo modsDirectory = new DirectoryInfo(modsDir);

                foreach (FileInfo file in modsDirectory.GetFiles("DistantHorizons*.jar"))
                {
                    string newFileName = file.FullName + ".disabled";
                    File.Move(file.FullName, newFileName);
                    if (logTextBox.Visible)
                    {
                        string message = $"Renombrado: {file.Name} a {Path.GetFileName(newFileName)}";
                        Invoke((MethodInvoker)(() =>
                        {
                            logTextBox.AppendText(message + Environment.NewLine);
                            logTextBox.ScrollToCaret();
                        }));
                    }
                }
            }

            if (downloadDistantHorizons)
            {
                // Ruta de la carpeta de descargas del usuario
                string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string distantFileToCheck = Path.Combine(downloadsFolder, "Distant_Horizons_server_data.zip");

                if (File.Exists(distantFileToCheck))
                {
                    // El archivo ya est� presente en la carpeta de descargas, proceder con la extracci�n
                    button2.Text = "Extrayendo Distant Horizons...";
                    // Asegurarse de que el cambio de texto se aplique antes de continuar
                    await Task.Delay(500); // Esperar medio segundo para asegurar la actualizaci�n

                    try
                    {
                        using (ZipFile zip = ZipFile.Read(distantFileToCheck))
                        {
                            zip.ExtractProgress += (sender, e) =>
                            {
                                if (e.EventType == ZipProgressEventType.Extracting_AfterExtractEntry)
                                {
                                    string message = $"Extrayendo Distant Horizons: {e.CurrentEntry.FileName}";
                                    Invoke((MethodInvoker)(() =>
                                    {
                                        logTextBox.AppendText(message + Environment.NewLine);
                                        logTextBox.ScrollToCaret();
                                    }));
                                }
                                else if (e.EventType == ZipProgressEventType.Extracting_EntryBytesWritten)
                                {
                                    int progress = (int)(e.BytesTransferred * 100 / e.TotalBytesToTransfer);
                                    Invoke((MethodInvoker)(() => progressBarUI.Value = progress));

                                    if (logTextBox.Visible)
                                    {
                                        string message = $"Extrayendo Distant Horizons: {e.BytesTransferred} / {e.TotalBytesToTransfer} bytes";
                                        Invoke((MethodInvoker)(() =>
                                        {
                                            logTextBox.AppendText(message + Environment.NewLine);
                                            logTextBox.ScrollToCaret();
                                        }));
                                    }
                                }
                            };
                            zip.ExtractAll(selectedPath, ExtractExistingFileAction.OverwriteSilently);
                            // FixDistant();
                        }

                        // Eliminar el archivo ZIP despu�s de la extracci�n
                        File.Delete(distantFileToCheck);

                        // Una vez completada la extracci�n, marcar distant.txt como "true"
                        File.WriteAllText(distantFilePath, "true");
                        MessageBox.Show("Distant Horizons se ha descargado e instalado correctamente.", "Instalaci�n Completa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error durante la extracci�n: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // El archivo no est� presente, abrir la p�gina de descarga
                    button2.Text = "Abriendo la p�gina de descarga...";
                    // Asegurarse de que el cambio de texto se aplique antes de continuar
                    await Task.Delay(500); // Esperar medio segundo para asegurar la actualizaci�n

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = distantHorizons_url,
                        UseShellExecute = true
                    });

                    // Mostrar un MessageBox para esperar la confirmaci�n del usuario
                    DialogResult result = MessageBox.Show(
                        "Se ha abierto la p�gina de descarga de Distant Horizons. Descargue el archivo ZIP y cuando termine pulse OK para continuar.",
                        "Esperando Confirmaci�n",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Information
                    );

                    if (result == DialogResult.OK)
                    {
                        // Comprobar nuevamente si el archivo se encuentra ahora en la carpeta de descargas
                        if (File.Exists(distantFileToCheck))
                        {
                            // El archivo ya se descarg� en la carpeta de descargas, proceder con la extracci�n
                            button2.Text = "Extrayendo Distant Horizons...";
                            await Task.Delay(500); // Esperar medio segundo para asegurar la actualizaci�n

                            try
                            {
                                using (ZipFile zip = ZipFile.Read(distantFileToCheck))
                                {
                                    zip.ExtractProgress += (sender, e) =>
                                    {
                                        if (e.EventType == ZipProgressEventType.Extracting_AfterExtractEntry)
                                        {
                                            string message = $"Extrayendo Distant Horizons: {e.CurrentEntry.FileName}";
                                            Invoke((MethodInvoker)(() =>
                                            {
                                                logTextBox.AppendText(message + Environment.NewLine);
                                                logTextBox.ScrollToCaret();
                                            }));
                                        }
                                        else if (e.EventType == ZipProgressEventType.Extracting_EntryBytesWritten)
                                        {
                                            int progress = (int)(e.BytesTransferred * 100 / e.TotalBytesToTransfer);
                                            Invoke((MethodInvoker)(() => progressBarUI.Value = progress));

                                            if (logTextBox.Visible)
                                            {
                                                string message = $"Extrayendo Distant Horizons: {e.BytesTransferred} / {e.TotalBytesToTransfer} bytes";
                                                Invoke((MethodInvoker)(() =>
                                                {
                                                    logTextBox.AppendText(message + Environment.NewLine);
                                                    logTextBox.ScrollToCaret();
                                                }));
                                            }
                                        }
                                    };
                                    zip.ExtractAll(selectedPath, ExtractExistingFileAction.OverwriteSilently);
                                    // FixDistant();
                                }

                                // Eliminar el archivo ZIP despu�s de la extracci�n
                                File.Delete(distantFileToCheck);

                                // Una vez completada la extracci�n, marcar distant.txt como "true"
                                File.WriteAllText(distantFilePath, "true");
                                MessageBox.Show("Distant Horizons se ha descargado e instalado correctamente.", "Instalaci�n Completa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error durante la extracci�n: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            // Si no se encuentra en la carpeta de descargas, abrir el cuadro de di�logo de selecci�n
                            button2.Text = "Esperando la selecci�n del archivo ZIP...";
                            await Task.Delay(500); // Esperar medio segundo para asegurar la actualizaci�n

                            using (OpenFileDialog openFileDialog = new OpenFileDialog())
                            {
                                openFileDialog.Filter = "Archivos ZIP (*.zip)|*.zip";
                                openFileDialog.Title = "Selecciona el archivo ZIP descargado";

                                if (openFileDialog.ShowDialog() == DialogResult.OK)
                                {
                                    string zipFilePath = openFileDialog.FileName;

                                    button2.Text = "Extrayendo Distant Horizons...";
                                    await Task.Delay(500); // Esperar medio segundo para asegurar la actualizaci�n

                                    try
                                    {
                                        using (ZipFile zip = ZipFile.Read(zipFilePath))
                                        {
                                            zip.ExtractProgress += (sender, e) =>
                                            {
                                                if (e.EventType == ZipProgressEventType.Extracting_AfterExtractEntry)
                                                {
                                                    string message = $"Extrayendo Distant Horizons: {e.CurrentEntry.FileName}";
                                                    Invoke((MethodInvoker)(() =>
                                                    {
                                                        logTextBox.AppendText(message + Environment.NewLine);
                                                        logTextBox.ScrollToCaret();
                                                    }));
                                                }
                                                else if (e.EventType == ZipProgressEventType.Extracting_EntryBytesWritten)
                                                {
                                                    int progress = (int)(e.BytesTransferred * 100 / e.TotalBytesToTransfer);
                                                    Invoke((MethodInvoker)(() => progressBarUI.Value = progress));

                                                    if (logTextBox.Visible)
                                                    {
                                                        string message = $"Extrayendo Distant Horizons: {e.BytesTransferred} / {e.TotalBytesToTransfer} bytes";
                                                        Invoke((MethodInvoker)(() =>
                                                        {
                                                            logTextBox.AppendText(message + Environment.NewLine);
                                                            logTextBox.ScrollToCaret();
                                                        }));
                                                    }
                                                }
                                            };
                                            zip.ExtractAll(selectedPath, ExtractExistingFileAction.OverwriteSilently);
                                            // FixDistant();
                                        }

                                        // Eliminar el archivo ZIP despu�s de la extracci�n
                                        File.Delete(zipFilePath);

                                        // Una vez completada la extracci�n, marcar distant.txt como "true"
                                        File.WriteAllText(distantFilePath, "true");
                                        MessageBox.Show("Distant Horizons se ha descargado e instalado correctamente.", "Instalaci�n Completa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show($"Error durante la extracci�n: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("No se seleccion� ning�n archivo. La instalaci�n de Distant Horizons se ha cancelado.", "Cancelado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("La instalaci�n de Distant Horizons se ha cancelado.", "Cancelado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }



            //////////////////////////////////
            // Reiniciar la barra de progreso
            /////////////////////////////////
            InitializeProgressBar();

            Cursor = Cursors.Default;
            button2.Text = "�Listo! Modpack instalado correctamente!";
            button1.Text = "Finalizado";
            MessageBox.Show("La instalaci�n del modpack oficial de OnePieceWorld ha finalizado. Ahora ejecute su Launcher y seleccione el perfil OnePieceWorld", "Instalaci�n Completada", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Remover los controles TextBox y enlace del formulario
            Controls.Remove(logTextBox);


            // Abrir form Ajustes y pasar los valores actuales
            Ajustes form2 = new Ajustes(selectedPath, ajustesCheckBoxValue, ajustesTrackBar1Value, ajustesTrackBar2Value);
            form2.ShowDialog();
        }

        private void CopyFilesIfNotExists(string sourceDir, string destDir)
        {
            // Verificar que el directorio fuente exista
            if (!Directory.Exists(sourceDir))
            {
                //El directorio de origen no existe
                return;
            }

            // Crear el directorio de destino si no existe
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // Obtener los archivos del directorio fuente
            string[] files = Directory.GetFiles(sourceDir);

            foreach (string file in files)
            {
                // Obtener el nombre del archivo sin la ruta
                string fileName = Path.GetFileName(file);
                // Ruta completa del archivo en el directorio destino
                string destFile = Path.Combine(destDir, fileName);

                // Verificar si el archivo ya existe en el directorio destino
                if (!File.Exists(destFile))
                {
                    // Copiar el archivo si no existe
                    File.Copy(file, destFile);
                }
                else
                {
                    //Archivo ya existe, no copiado
                }
            }

            // Obtener los subdirectorios del directorio fuente
            string[] dirs = Directory.GetDirectories(sourceDir);

            foreach (string dir in dirs)
            {
                // Obtener el nombre del subdirectorio sin la ruta
                string dirName = Path.GetFileName(dir);
                // Ruta completa del subdirectorio en el directorio destino
                string destSubDir = Path.Combine(destDir, dirName);

                // Copiar recursivamente los archivos y subdirectorios
                CopyFilesIfNotExists(dir, destSubDir);
            }
        }



        private void SaveProgramVersion()
        {
            try
            {
                // Guardar versi�n del programa
                string programFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft", "version.txt");
                File.WriteAllText(programFilePath, program_version);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("No se puede guardar la versi�n actual del programa.", "Error de acceso", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveModpackVersion()
        {
            try
            {
                // Guardar versi�n del modpack
                string installedFilePath = Path.Combine(selectedPath, "modpack_version.txt");
                File.WriteAllText(installedFilePath, modpack_version);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("No se puede guardar la versi�n actual del programa.", "Error de acceso", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_MouseEnter(object sender, EventArgs e)
        {
            button2.BackgroundImage = Properties.Resources.widgetss1;
        }

        private void button2_MouseLeave(object sender, EventArgs e)
        {
            button2.BackgroundImage = Properties.Resources.widgets1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Selecciona una instancia para el modpack";
            dialog.RootFolder = Environment.SpecialFolder.ApplicationData;
            dialog.ShowNewFolderButton = false;

            // Establecer la ubicaci�n inicial del explorador de archivos
            dialog.SelectedPath = selectedPath; // Reemplaza "selectedPath" con la ubicaci�n deseada

            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                selectedPath = dialog.SelectedPath;
                button2.Text = selectedPath;
            }
        }

        private void pictureBox5_Click(object? sender, EventArgs e)
        {
            selectedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft", "_OnePieceWorld");
            button2.Text = selectedPath;
            SaveCustomPath();
        }
        private void PictureBox5_MouseEnter(object? sender, EventArgs e)
        {
            pictureBox5.Image = Properties.Resources.restart_b;
        }
        private void PictureBox5_MouseLeave(object? sender, EventArgs e)
        {
            pictureBox5.Image = Properties.Resources.restart;
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            string installedFilePath = Path.Combine(selectedPath, "installed.txt");

            if (File.Exists(installedFilePath) && File.ReadAllText(installedFilePath).Trim() == "OK")
            {
                // Abrir form Ajustes y pasar los valores actuales
                Ajustes form2 = new Ajustes(selectedPath, ajustesCheckBoxValue, ajustesTrackBar1Value, ajustesTrackBar2Value);
                form2.ShowDialog();
            }
            else
            {
                MessageBox.Show("El modpack no est� instalado correctamente. Por favor, realice la instalaci�n antes de acceder a los ajustes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox6_MouseEnter(object sender, EventArgs e)
        {
            // Cambiar el tama�o de la imagen al entrar el cursor
            pictureBox6.Size = new Size(pictureBox6.Width + 10, pictureBox6.Height + 10);
        }

        private void pictureBox6_MouseLeave(object sender, EventArgs e)
        {
            // Restaurar el tama�o original de la imagen al salir el cursor
            pictureBox6.Size = new Size(pictureBox6.Width - 10, pictureBox6.Height - 10);
        }


        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Esto eliminar� el modpack incluido todas sus configuraciones. �Seguro que quiere continuar?", "Advertencia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                Cursor = Cursors.WaitCursor;
                button2.Text = "Desinstalando Modpack...";
                // Borrar carpeta del modpack de la ruta elegida por el usuario
                if (Directory.Exists(selectedPath))
                {
                    // Elimina la carpeta y todos sus contenidos
                    Directory.Delete(selectedPath, true);
                }

                // Leer el contenido existente del archivo JSON
                string perfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft", "launcher_profiles.json");
                var json = File.ReadAllText(perfilesPath);
                var jsonObj = JObject.Parse(json);

                // La variable para almacenar el nombre del perfil a eliminar
                string perfilNombre = "OnePieceWorld";
                bool perfilEliminado = false;

                // Verificar si el JSON tiene la propiedad "profiles"
                if (jsonObj["profiles"] is JObject profiles)
                {
                    // Buscar el perfil con el nombre espec�fico en "profiles"
                    foreach (var kvp in profiles)
                    {
                        var perfil = kvp.Value as JObject;
                        if (perfil != null && perfil["name"]?.ToString() == perfilNombre)
                        {
                            // Eliminar el perfil encontrado
                            profiles.Remove(kvp.Key);
                            perfilEliminado = true;
                            break;
                        }
                    }
                }

                // Si el perfil fue eliminado, escribir el JSON modificado de vuelta en el archivo
                if (perfilEliminado)
                {
                    File.WriteAllText(perfilesPath, jsonObj.ToString(Formatting.Indented));
                }



                // Construir la ruta hacia la carpeta "OnePieceWorld" dentro de "versions"
                string OnePieceWorldPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft", "versions", "OnePieceWorld");

                // Verificar si la carpeta existe antes de intentar eliminarla
                if (Directory.Exists(OnePieceWorldPath))
                {
                    // Eliminar la carpeta "OnePieceWorld" y todo su contenido
                    Directory.Delete(OnePieceWorldPath, true);
                }

                button2.Text = "�Modpack Desinstalado correctamente!";
                button3.Text = "DESINSTALADO";
                MessageBox.Show("Desinstalaci�n completada con �xito", "Informaci�n", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cursor = Cursors.Default;
            }
            else if (result == DialogResult.No)
            {
                // El usuario hizo clic en Rechazar, se cancela el proceso
                MessageBox.Show("Proceso cancelado satisfactoriamente", "Informaci�n", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }



        private void button3_MouseEnter(object? sender, EventArgs e)
        {
            button3.BackgroundImage = Properties.Resources.half_widgetss4;
        }
        private void button3_MouseLeave(object? sender, EventArgs e)
        {
            button3.BackgroundImage = Properties.Resources.half_widgets4;
        }

    }
}