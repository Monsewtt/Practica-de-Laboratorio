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
        public Form1()
        {
            InitializeComponent();
        }

        private void btnMostrar_Click(object sender, EventArgs e)
        {
            string miIP = "192.168.1.113";
            string cadena = $@"Server={miIP}\SQLEXPRESS; Database=PracticaLab; User Id=sa; Password=123; TrustServerCertificate=True;";

            using (SqlConnection conexion = new SqlConnection(cadena))
            {
                try
                {
                    conexion.Open();
                    string query = "SELECT * FROM Ciudadanos";

                    SqlDataAdapter adaptador = new SqlDataAdapter(query, conexion);
                    DataTable dt = new DataTable();
                    adaptador.Fill(dt);
                    dataGridView1.DataSource = dt;

                    MessageBox.Show("¡Datos cargados desde la otra PC!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error de red: " + ex.Message);
                }
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            // Usa la misma IP y credenciales que en tu botón de Mostrar
            string miIP = "192.168.1.113";
            string cadena = $@"Server={miIP}\SQLEXPRESS; Database=PracticaLab; User Id=sa; Password=123; TrustServerCertificate=True;";

            using (SqlConnection conexion = new SqlConnection(cadena))
            {
                try
                {
                    conexion.Open();

                    // 1. Le decimos de qué tabla sacamos los datos originalmente
                    SqlDataAdapter adaptador = new SqlDataAdapter("SELECT * FROM Ciudadanos", conexion);

                    // 2. MAGIA: Esto genera los comandos de guardar en automático
                    SqlCommandBuilder constructor = new SqlCommandBuilder(adaptador);

                    // 3. Rescatamos la tabla con los datos que tú ya editaste en la pantalla
                    DataTable tablaEditada = (DataTable)dataGridView1.DataSource;

                    // 4. Mandamos todos los cambios de golpe a SQL
                    adaptador.Update(tablaEditada);

                    MessageBox.Show("¡Cambios guardados con éxito en la base de datos!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar: " + ex.Message);
                }
            }
    }
    }
}
