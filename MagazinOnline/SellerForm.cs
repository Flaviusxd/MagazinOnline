using MagazinOnline.Clase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MagazinOnline
{
    public partial class SellerForm : Form
    {
        private Seller vanzatorCurent;
        private List<Product> produse;
        private List<Offer> oferte;

        public SellerForm(Seller vanzator)
        {
            InitializeComponent();
            this.vanzatorCurent = vanzator;
            IncarcaProduse();
            AfiseazaProduse();
            IncarcaOferte();
            AfiseazaOferta();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnCereAprobare_Click(object sender, EventArgs e)
        {
            vanzatorCurent.IsApproved = true; // presupunem că trimite cererea automat
            SalveazaVanzator();
            MessageBox.Show("Cererea de aprobare a fost trimisă.");
        }

        private void btnAdaugaProdus_Click(object sender, EventArgs e)
        {
            var produs = new Product
            {
                Id = GetNextProductId(),
                Name = txtNume.Text,
                Price = numPret.Value,
                Description = txtDescriere.Text,
                EsteNegociabil = chkNegociabil.Checked,
                VanzatorEmail = vanzatorCurent.Email
            };

            produse.Add(produs);
            SalveazaProduse();
            AfiseazaProduse();
            MessageBox.Show("Produs adăugat.");
        }

        private void btnAnuleazaVanzare_Click(object sender, EventArgs e)
        {
            if (dataGridViewProduse.CurrentRow != null)
            {
                int id = Convert.ToInt32(dataGridViewProduse.CurrentRow.Cells["Id"].Value);
                var produs = produse.FirstOrDefault(p => p.Id == id && p.VanzatorEmail == vanzatorCurent.Email);
                if (produs != null)
                {
                    produse.Remove(produs);
                    SalveazaProduse();
                    AfiseazaProduse();
                    MessageBox.Show("Produsul a fost anulat.");
                }
            }
        }

        private void btnAprobaNegociere_Click(object sender, EventArgs e)
        {
            // Ex: setează preț negociat = preț curent - 10%
            if (dataGridViewProduse.CurrentRow != null)
            {
                int id = Convert.ToInt32(dataGridViewProduse.CurrentRow.Cells["Id"].Value);
                var produs = produse.FirstOrDefault(p => p.Id == id && p.EsteNegociabil);
                if (produs != null)
                {
                    produs.Price *= 0.9m; // exemplu: reducere 10%
                    SalveazaProduse();
                    AfiseazaProduse();
                    MessageBox.Show("Preț negociat aprobat.");
                }
            }
        }

        private void btnAnuleazaNegociere_Click(object sender, EventArgs e)
        {
            // Marcare ca non-negociabil
            if (dataGridViewProduse.CurrentRow != null)
            {
                int id = Convert.ToInt32(dataGridViewProduse.CurrentRow.Cells["Id"].Value);
                var produs = produse.FirstOrDefault(p => p.Id == id && p.EsteNegociabil);
                if (produs != null)
                {
                    produs.EsteNegociabil = false;
                    SalveazaProduse();
                    AfiseazaProduse();
                    MessageBox.Show("Negocierea a fost anulată.");
                }
            }
        }

        private void IncarcaProduse()
        {
            string path = "Produse/produse.json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                produse = JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
            }
            else
            {
                produse = new List<Product>();
            }
        }

        private void SalveazaProduse()
        {
            string path = "Produse/produse.json";
            string json = JsonSerializer.Serialize(produse, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        private void SalveazaVanzator()
        {
            string path = "Conturi/sellers.json";
            var totiVanzatorii = JsonSerializer.Deserialize<List<Seller>>(File.ReadAllText(path));
            var index = totiVanzatorii.FindIndex(v => v.Email == vanzatorCurent.Email);
            if (index >= 0)
            {
                totiVanzatorii[index] = vanzatorCurent;
                File.WriteAllText(path, JsonSerializer.Serialize(totiVanzatorii, new JsonSerializerOptions { WriteIndented = true }));
            }
        }

        private void AfiseazaProduse()
        {
            dataGridViewProduse.DataSource = produse
                .Where(p => p.VanzatorEmail == vanzatorCurent.Email)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Description,
                    Negociabil = p.EsteNegociabil ? "Da" : "Nu"
                }).ToList();
        }

        private void IncarcaOferte()
        {
            string path = "Oferte/oferte.json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                oferte = JsonSerializer.Deserialize<List<Offer>>(json) ?? new List<Offer>();
            }
            else
            {
                oferte = new List<Offer>();
            }
        }

        private void SalveazaOferta()
        {
            string path = "Oferte/oferte.json";
            string json = JsonSerializer.Serialize(oferte, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        private void AfiseazaOferta()
        {
            dataGridViewOferte.Rows.Clear();
            dataGridViewOferte.Columns.Clear();

            string sellerEmail = vanzatorCurent.Email; // sau unde ții emailul vânzătorului logat

            var produse = JsonSerializer.Deserialize<List<Product>>(File.ReadAllText("Produse/produse.json"));
            var oferte = JsonSerializer.Deserialize<List<Offer>>(File.ReadAllText("Oferte/oferte.json"));

            // Faci join între oferte și produse pentru a obține numele produsului
            var oferteAfisate = from oferta in oferte
                                join produs in produse on oferta.ProductId equals produs.Id
                                where produs.VanzatorEmail == sellerEmail
                                select new
                                {
                                    NumeProdus = produs.Name,
                                    Cumparator = oferta.BuyerEmail,
                                    Pret = oferta.OfferPrice
                                };

            dataGridViewOferte.Columns.Add("Produs", "Produs");
            dataGridViewOferte.Columns.Add("Cumparator", "Cumparator");
            dataGridViewOferte.Columns.Add("Pret", "Preț propus");

            foreach (var item in oferteAfisate)
            {
                dataGridViewOferte.Rows.Add(item.NumeProdus, item.Cumparator, item.Pret);
            }
        }


        private int GetNextProductId()
        {
            return produse.Any() ? produse.Max(p => p.Id) + 1 : 1;
        }

        private void SellerForm_Load(object sender, EventArgs e)
        {
            AfiseazaOferta();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void lblBunVenit_Click(object sender, EventArgs e)
        {

        }

        private void SellerForm_Load_1(object sender, EventArgs e)
        {

        }

        private void dataGridViewProduse_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {
            AfiseazaOferta();
        }

        private void dataGridViewOferte_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            AfiseazaOferta();
        }
    }
}
