using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SignatureTestExample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sigPlusNET1.SetTabletState(1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            sigPlusNET1.ClearTablet();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string sigString = sigPlusNET1.GetSigString();
                // MessageBox.Show(sigString);

                string constring = @"Data Source=DESKTOP-FJBB72F\SQLEXPRESS;Initial Catalog=SignatureCapture;Integrated Security=True";
                using (SqlConnection con = new SqlConnection(constring))
                {

                    con.Open();
                    string query = "insert into SignTable (signature) values (@signature)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@signature", sigString);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("OK I Am inserted!");
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.ToString());
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string constring = @"Data Source=DESKTOP-FJBB72F\SQLEXPRESS;Initial Catalog=SignatureCapture;Integrated Security=True";
            using (SqlConnection con = new SqlConnection(constring))
            {
                con.Open();
                string sql = "select * from SignTable where id =@id";
                using (SqlCommand cm = new SqlCommand(sql, con))
                {
                    cm.Parameters.AddWithValue("@id", textBox1.Text);
                    try
                    {
                        using (SqlDataReader rd = cm.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                // byte[] imgData = (byte[])rd["signature"];
                                //byte[] imgData = Convert.FromBase64String(rd["signature"].ToString());
                                byte[] imgData = Encoding.ASCII.GetBytes(rd["signature"].ToString());
                                using (MemoryStream ms = new MemoryStream(imgData))
                                {
                                    System.Drawing.Image image = Image.FromStream(ms);
                                    //image.Save(@"C:\Users\Administrator\Desktop\UserPhoto.jpg");
                                    pictureBox1.Image = image;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.ToString());
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            sigPlusNET1.SetTabletState(0); //tablet off 
            sigPlusNET1.SetImageXSize(1000);
            sigPlusNET1.SetImageYSize(300);
            sigPlusNET1.SetJustifyY(10);
            sigPlusNET1.SetJustifyX(10);
            sigPlusNET1.SetJustifyMode(5);
            sigPlusNET1.SetImagePenWidth(10);
            sigPlusNET1.SetImageFileFormat(4); //0=bmp, 4=jpg, 6=tif
            Image sigImage = sigPlusNET1.GetSigImage();
            String sigBase64 = ImageToBase64(sigImage, ImageFormat.Jpeg);            
            string sigString = sigPlusNET1.GetSigString();

            if (sigBase64.Length > 2500)
            {
                string constring = @"Data Source=DESKTOP-FJBB72F\SQLEXPRESS;Initial Catalog=SignatureProcess;Integrated Security=True";
                using (SqlConnection con = new SqlConnection(constring))
                {
                    con.Open();
                    try
                    {
                        string insertSql = "insert into signatureProcessNew (sigString,base64Sigstring) values (@sigString,@base64Sigstring)";
                        using (SqlCommand cd = new SqlCommand(insertSql, con))
                        {
                            cd.Parameters.AddWithValue("@sigString", sigString);
                            cd.Parameters.AddWithValue("@base64Sigstring", sigBase64);
                            cd.ExecuteNonQuery();
                            MessageBox.Show("Data Saved!");
                            con.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.ToString());
                    }
                }
            }
        }

        public string ImageToBase64(Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[] 
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();
                // Convert byte[] to Base64 String 
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }
    }
}
