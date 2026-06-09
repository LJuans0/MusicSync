Imports MediaDevices
Imports System.Management
Imports System.Threading.Tasks
Class Musicsync

    Private Sub Window_Loaded2(sender As Object,
                          e As RoutedEventArgs) _
                          Handles Me.Loaded

        TxtCarpeta.Text = carpetaMusica

        IniciarDeteccionUSB()

        BuscarAndroid()

    End Sub

    Private watcher As ManagementEventWatcher
    Private Sub IniciarDeteccionUSB()
        watcher = New ManagementEventWatcher()

        watcher.Query = New WqlEventQuery(
        "SELECT * FROM Win32_DeviceChangeEvent")

        AddHandler watcher.EventArrived,
        AddressOf USB_Cambiado

        watcher.Start()

    End Sub

    Private Sub USB_Cambiado(sender As Object,
                         e As EventArrivedEventArgs)

        Dispatcher.Invoke(Sub()

                              BuscarAndroid()

                          End Sub)

    End Sub

    Private Sub BuscarAndroid()

        Try

            Dim devices = MediaDevice.GetDevices()

            If devices.Count = 0 Then

                TxtDispositivo.Text = "No conectado"
                TxtCarpeta2.Text = "No disponible"
                carpetaAndroid = ""
                CmbCarpetasAndroid.Items.Clear()
                CmbCarpetasAndroid.Visibility = Visibility.Collapsed
                Dispatcher.Invoke(
    Sub()

        PrgSync.Value = 0

    End Sub)

                Dispatcher.Invoke(
    Sub()

        LstRegistro.Items.Clear()


    End Sub)


                Dispatcher.Invoke(
    Sub()

        TxtAccionActual.Text =
            "Esperando a copiar..."

    End Sub)
                Exit Sub

            End If

            For Each device In devices

                device.Connect()

                TxtDispositivo.Text =
                device.Model

                BuscarCarpetaMusica(device)

                device.Disconnect()

                Exit Sub

            Next

            TxtDispositivo.Text =
            "Esperando a que lo conectes"

        Catch ex As Exception

            TxtDispositivo.Text = "No conectado"
            TxtCarpeta2.Text = "No disponible"
            carpetaAndroid = ""
            CmbCarpetasAndroid.Items.Clear()
            CmbCarpetasAndroid.Visibility = Visibility.Collapsed
            Dispatcher.Invoke(
    Sub()

        PrgSync.Value = 0

    End Sub)

            Dispatcher.Invoke(
    Sub()

        LstRegistro.Items.Clear()

    End Sub)


            Dispatcher.Invoke(
    Sub()

        TxtAccionActual.Text =
            "Preparando para copiar..."

    End Sub)

        End Try

    End Sub

    Private carpetaAndroid As String = ""

    Private Sub BuscarCarpetaMusica(device As MediaDevice)

        Try

            Dim posiblesRutas As String() =
        {
            "\Internal storage\Music",
            "\Phone\Music",
            "\Music",
            "\Almacenamiento interno compartido\Music"
        }

            For Each ruta In posiblesRutas

                If device.DirectoryExists(ruta) Then

                    carpetaAndroid = ruta
                    TxtCarpeta2.Text = ruta

                    Exit Sub

                End If

            Next

            TxtCarpeta2.Text =
            "No encontrada"

        Catch ex As Exception

            TxtCarpeta2.Text =
            "Error buscando Music"

        End Try

    End Sub

    Private Sub BtnCambiarCarpeta2_Click(sender As Object,
                                     e As RoutedEventArgs) _
                                     Handles BtnCambiarCarpeta2.Click

        Try

            Dim devices = MediaDevice.GetDevices()

            If devices.Count = 0 Then

                Forms.MessageBox.Show("No hay dispositivo conectado")
                Exit Sub

            End If

            Dim device = devices.First()

            device.Connect()

            CmbCarpetasAndroid.Items.Clear()

            Dim carpetas =
            device.EnumerateDirectories(
                "\Almacenamiento interno compartido")

            For Each carpeta In carpetas

                Dim nombre As String =
                IO.Path.GetFileName(carpeta)

                CmbCarpetasAndroid.Items.Add(carpeta)

            Next

            device.Disconnect()

            CmbCarpetasAndroid.Visibility =
            Visibility.Visible

        Catch ex As Exception

            Forms.MessageBox.Show(ex.Message)

        End Try

    End Sub

    Private Sub CmbCarpetasAndroid_SelectionChanged(
    sender As Object,
    e As SelectionChangedEventArgs) _
    Handles CmbCarpetasAndroid.SelectionChanged

        If CmbCarpetasAndroid.SelectedItem IsNot Nothing Then

            carpetaAndroid =
            CmbCarpetasAndroid.SelectedItem.ToString()

            TxtCarpeta2.Text =
            carpetaAndroid

        End If

    End Sub


    Private carpetaMusica As String =
        Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)
    Private totalArchivos As Integer = 0
    Private archivosCopiados As Integer = 0
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) _
        Handles Me.Loaded

        TxtCarpeta.Text = carpetaMusica

    End Sub

    Private Sub BtnCambiarCarpeta_Click(sender As Object,
                                    e As RoutedEventArgs)


        Using dialog As New FolderBrowserDialog()

            dialog.SelectedPath = carpetaMusica

            If dialog.ShowDialog() = Forms.DialogResult.OK Then

                carpetaMusica = dialog.SelectedPath
                TxtCarpeta.Text = carpetaMusica

            End If

        End Using

    End Sub

    Private Function ObtenerCarpetasPC() As List(Of String)

        Dim resultado As New List(Of String)

        Try

            For Each carpeta In IO.Directory.GetDirectories(carpetaMusica)

                resultado.Add(
                IO.Path.GetFileName(carpeta)
            )

            Next

        Catch ex As Exception

        End Try

        Return resultado

    End Function

    Private Function ObtenerCarpetasAndroid() As List(Of String)

        Dim resultado As New List(Of String)

        Try

            Dim devices = MediaDevice.GetDevices()

            If devices.Count = 0 Then Return resultado

            Dim device = devices.First()

            device.Connect()

            Dim carpetas =
                device.EnumerateDirectories(carpetaAndroid)

            For Each carpeta In carpetas

                resultado.Add(
                    IO.Path.GetFileName(carpeta)
                )

            Next

            device.Disconnect()

        Catch ex As Exception

        End Try

        Return resultado

    End Function

    Private Function ObtenerCarpetasFaltantes() As List(Of String)

        Dim android = ObtenerCarpetasAndroid()

        Dim pc = ObtenerCarpetasPC()

        Return android.
            Except(pc,
            StringComparer.OrdinalIgnoreCase).
            ToList()

    End Function

    Private Sub BtnComparar_Click(sender As Object,
                              e As RoutedEventArgs) _
                              Handles BtnComparar.Click

        Dim faltantes =
            ObtenerCarpetasFaltantes()

        If faltantes.Count = 0 Then

            Forms.MessageBox.Show(
                "No hay carpetas nuevas")

            Return

        End If

        Forms.MessageBox.Show(
            String.Join(vbCrLf, faltantes))

    End Sub

    Private Async Sub BtnSincronizar_Click(sender As Object,
                                 e As RoutedEventArgs) _
                                 Handles BtnCopiar.Click

        Dim faltantes =
            ObtenerCarpetasFaltantes()


        If faltantes.Count = 0 Then

            Forms.MessageBox.Show(
                "No hay carpetas nuevas")

            Return

        End If

        BtnCopiar.IsEnabled = False

        Await Task.Run(
    Sub()

        CopiarCarpetasAndroid(
            faltantes)

    End Sub)

        Forms.MessageBox.Show(
            "yupiiii!!! ya esta :v")
        BtnCopiar.IsEnabled = True
    End Sub

    Private Sub CopiarCarpetasAndroid(
    carpetas As List(Of String))

        totalArchivos = 0
        archivosCopiados = 0

        AgregarRegistro(
    "Calculando archivos...")

        Dim devices = MediaDevice.GetDevices()

        If devices.Count = 0 Then Exit Sub

        Dim device = devices.First()

        device.Connect()

        For Each nombreCarpeta In carpetas

            Dim origenAndroid =
                carpetaAndroid & "\" & nombreCarpeta

            Dim destinoPC =
                IO.Path.Combine(
                    carpetaMusica,
                    nombreCarpeta)

            totalArchivos +=
            ContarArchivos(
            device,
            origenAndroid)
            AgregarRegistro(
    $"{totalArchivos} archivos encontrados")
            CopiarCarpetaRecursiva(
                device,
                origenAndroid,
                destinoPC)

        Next
        AgregarRegistro(
    "Sincronización completada")

        Dispatcher.Invoke(
    Sub()

        TxtAccionActual.Text =
            "Finalizado!"

    End Sub)
        Dispatcher.Invoke(
    Sub()

        PrgSync.Value = 100

    End Sub)
        device.Disconnect()

    End Sub

    Private Sub CopiarCarpetaRecursiva(
    device As MediaDevice,
    origenAndroid As String,
    destinoPC As String)

        If Not IO.Directory.Exists(destinoPC) Then

            IO.Directory.CreateDirectory(
                destinoPC)

        End If

        For Each archivo In
            device.EnumerateFiles(origenAndroid)

            Dim nombreArchivo =
                IO.Path.GetFileName(archivo)

            Dim destinoArchivo =
                IO.Path.Combine(
                    destinoPC,
                    nombreArchivo)

            Dispatcher.Invoke(
    Sub()

        TxtAccionActual.Text =
            nombreArchivo

    End Sub)

            AgregarRegistro(
    $"Copiando {nombreArchivo}")

            device.DownloadFile(
                archivo,
                destinoArchivo)

            archivosCopiados += 1

            ActualizarProgreso(
                archivosCopiados,
                totalArchivos)

        Next

        For Each subcarpeta In
            device.EnumerateDirectories(origenAndroid)

            Dim nombreSubcarpeta =
                IO.Path.GetFileName(subcarpeta)

            Dim destinoSubcarpeta =
                IO.Path.Combine(
                    destinoPC,
                    nombreSubcarpeta)

            CopiarCarpetaRecursiva(
                device,
                subcarpeta,
                destinoSubcarpeta)

        Next

    End Sub

    Private Sub AgregarRegistro(texto As String)

        Dispatcher.Invoke(Sub()

                              LstRegistro.Items.Add(
                                  $"[{DateTime.Now:HH:mm:ss}] {texto}")

                              LstRegistro.ScrollIntoView(
                                  LstRegistro.Items(LstRegistro.Items.Count - 1))

                          End Sub)

    End Sub
    Private Sub ActualizarProgreso(
    actual As Integer,
    total As Integer)

        Dispatcher.Invoke(Sub()

                              If total > 0 Then

                                  Dispatcher.Invoke(
    Sub()

        PrgSync.Value = (actual / total) * 100

    End Sub)



                              End If

                          End Sub)

    End Sub
    Private Function ContarArchivos(
    device As MediaDevice,
    ruta As String) As Integer

        Dim total As Integer = 0

        Try

            total += device.EnumerateFiles(ruta).Count()

            For Each carpeta In
                device.EnumerateDirectories(ruta)

                total += ContarArchivos(
                    device,
                    carpeta)

            Next

        Catch
        End Try

        Return total

    End Function
End Class