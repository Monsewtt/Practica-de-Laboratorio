using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
namespace Practica_de_Laboratorio
{
    public partial class Form1 : Form
    {
        // Dirección IP del servidor SQL Server
        string ipServidor = "192.168.56.1";
        public Form1()
        {
            InitializeComponent();
        }

        private void btnMostrar_Click(object sender, EventArgs e)
        {
            //cadena de conexion a la base de datos, utilizando el formato de cadena de conexion para SQL Server
            string cadena = $@"Server={ipServidor}\SQLEXPRESS; Database=PracticaLab; User Id=sa; Password=123; TrustServerCertificate=True;";
            // El bloque using asegura que la conexión se cierre correctamente 
            using (SqlConnection conexion = new SqlConnection(cadena))
            {
                try
                {
                    //abre la conexion a la base de datos
                    conexion.Open();
                    //consulta para obtener los datos de la tabla Ciudadanos
                    string query = "SELECT * FROM Ciudadanos";
                    //crea un adaptador para ejecutar la consulta y llenar un DataTable con los resultados
                    SqlDataAdapter adaptador = new SqlDataAdapter(query, conexion);
                    DataTable dt = new DataTable();
                    //llena el DataTable con los datos obtenidos de la consulta
                    adaptador.Fill(dt);
                    //vincula el DataTable al DataGridView para mostrar los datos en la interfaz de usuario
                    dataGridView1.DataSource = dt;

                    MessageBox.Show("Datos cargados");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error de red: " + ex.Message);
                }
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string cadena = $@"Server={ipServidor}\SQLEXPRESS; Database=PracticaLab; User Id=sa; Password=123; TrustServerCertificate=True;";

            using (SqlConnection conexion = new SqlConnection(cadena))
            {
                try
                {
                    conexion.Open();
                    SqlDataAdapter adaptador = new SqlDataAdapter("SELECT * FROM Ciudadanos", conexion);
                    // El SqlCommandBuilder se utiliza para generar automáticamente las sentencias SQL necesarias para actualizar la base de datos
                    SqlCommandBuilder constructor = new SqlCommandBuilder(adaptador);
                    // Obtiene el DataTable que está actualmente vinculado al DataGridView, que contiene los datos editados por el usuario
                    DataTable tablaEditada = (DataTable)dataGridView1.DataSource;
                    // El método Update del adaptador se encarga de generar las sentencias SQL necesarias para reflejar los cambios realizados en el DataTable
                    adaptador.Update(tablaEditada);

                    MessageBox.Show("Guardado");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar: " + ex.Message);
                }
            }
        }

        private void btnMigrar_Click(object sender, EventArgs e)
        {
            string cadena = $@"Server={ipServidor}\SQLEXPRESS; Database=PracticaLab; User Id=sa; Password=123; TrustServerCertificate=True;";
            GestorArchivos gestor = new GestorArchivos();

            using (SqlConnection conexion = new SqlConnection(cadena))
            {
                try
                {
                    conexion.Open();
                    richTextBox1.AppendText("\n> INICIANDO MIGRACIÓN A LA NUBE...\n");

                    int posicion = 0;
                    int migrados = 0;

                    while (true)
                    {
                        Ciudadano? c = gestor.LeerCiudadano(posicion);

                        if (c == null) break;

                        // ¡EL CAMBIO! Ya no nos importa el ID 0. 
                        // Solo revisamos que el nombre tenga letras y no esté en blanco.
                        string nombreLimpio = c.Value.Nombre.Trim('\0', ' ');

                        if (!string.IsNullOrEmpty(nombreLimpio))
                        {
                            string query = "INSERT INTO Ciudadanos (Nombre, Edad) VALUES (@Nombre, @Edad)";

                            using (SqlCommand cmd = new SqlCommand(query, conexion))
                            {
                                cmd.Parameters.AddWithValue("@Nombre", nombreLimpio);
                                cmd.Parameters.AddWithValue("@Edad", c.Value.Edad);
                                cmd.ExecuteNonQuery();
                            }
                            migrados++;
                            richTextBox1.AppendText($"> MIGRADO: {nombreLimpio}\n");
                        }
                        posicion++;
                    }

                    richTextBox1.AppendText($"> ¡MIGRACIÓN COMPLETADA! ({migrados} registros enviados)\n");
                    MessageBox.Show("¡Migración exitosa! Dale clic a Mostrar.");
                }
                catch (Exception ex)
                {
                    richTextBox1.AppendText($"> ERROR DE MIGRACIÓN: {ex.Message}\n");
                }
            }
        }

        private void btnGuardarArchivo_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. OBTENER DATOS
                string nombre = txtNombre.Text;
                int edad = int.Parse(txtEdad.Text);
                int posicion;

                // Si no pones posición, lo manda al final solito
                if (string.IsNullOrWhiteSpace(txtPosicion.Text))
                {
                    string ruta = "datos_ciudadanos.dat";
                    posicion = System.IO.File.Exists(ruta)
                        ? (int)(new System.IO.FileInfo(ruta).Length / Ciudadano.Size)
                        : 0;
                }
                else { posicion = int.Parse(txtPosicion.Text); }

                // 2. GUARDAR EN ARCHIVO .DAT (Nivel 1)
                GestorArchivos gestor = new GestorArchivos();
                Ciudadano c = new Ciudadano(posicion + 1, nombre, edad);
                gestor.GuardarCiudadano(c, posicion);

                // 3. GUARDAR EN SQL SERVER Y REFRESCAR TABLA (Nivel 3)
                string cadena = $@"Server={ipServidor}\SQLEXPRESS; Database=PracticaLab; User Id=sa; Password=123; TrustServerCertificate=True;";

                using (SqlConnection conexion = new SqlConnection(cadena))
                {
                    conexion.Open();
                    // Insertamos
                    string queryInsert = "INSERT INTO Ciudadanos (Nombre, Edad) VALUES (@Nombre, @Edad)";
                    using (SqlCommand cmd = new SqlCommand(queryInsert, conexion))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", nombre);
                        cmd.Parameters.AddWithValue("@Edad", edad);
                        cmd.ExecuteNonQuery();
                    }

                    // Refrescamos el cuadro gris automáticamente
                    SqlDataAdapter adaptador = new SqlDataAdapter("SELECT * FROM Ciudadanos", conexion);
                    DataTable dt = new DataTable();
                    adaptador.Fill(dt);
                    dataGridView1.DataSource = dt;
                }

                // 4. MENSAJE EN LA CONSOLA NEGRA
                richTextBox1.AppendText($"\n> [OK] {nombre} guardado en .DAT (Pos: {posicion}) y en SQL Server.");

                // Limpiamos las cajitas
                txtNombre.Clear();
                txtEdad.Clear();
                txtPosicion.Clear();
                txtNombre.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("¡Ups! Algo salió mal: " + ex.Message);
            }
        }

        private void btnBuscarID_Click(object sender, EventArgs e)
        {
            try
            {
                int posicion = int.Parse(txtPosicion.Text);
                GestorArchivos gestor = new GestorArchivos();

                // Le pedimos al gestor que lea esa posición específica
                Ciudadano? c = gestor.LeerCiudadano(posicion);

                if (c != null && c.Value.Id != 0)
                {
                    // Si sí encontró a alguien, llenamos las cajitas de text
                    txtNombre.Text = c.Value.Nombre;
                    txtEdad.Text = c.Value.Edad.ToString();

                    richTextBox1.AppendText($"> LECTURA: Se encontró a '{c.Value.Nombre}' en la posición {posicion}\n");
                }
                else
                {
                    richTextBox1.AppendText($"> ERROR: La posición {posicion} está vacía.\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Pon un número en la cajita de Posición. Error: " + ex.Message);
            }
        }

        private void txtId_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtNombre_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtEdad_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtPosicion_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
          
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            try
            {
                int posicion = int.Parse(txtPosicion.Text);
                GestorArchivos gestor = new GestorArchivos();
                Ciudadano? c = gestor.LeerCiudadano(posicion);
                richTextBox1.AppendText($"\n> LEYENDO POSICIÓN {posicion} DEL DISCO...\n");
                if (c != null && c.Value.Id != 0)
                {
                    int byteExacto = posicion * Ciudadano.Size;

                    richTextBox1.AppendText($"> ¡Datos encontrados en el byte {byteExacto}!\n");
                    richTextBox1.AppendText($"> - Nombre: {c.Value.Nombre.Trim()}\n");
                    richTextBox1.AppendText($"> - Edad: {c.Value.Edad} años\n");
                }
                else
                {
                    richTextBox1.AppendText($"> [VACÍO] No hay datos en la posición {posicion}.\n");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Por favor, escribe un número en la cajita de Posición antes de leer.");
            }
        }
    }
}
