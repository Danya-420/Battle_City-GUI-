using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Battle_City
{
    // Клас, який представляє головне меню гри
    public partial class MainMenuForm : Form
    {
        // Конструктор класу
        public MainMenuForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen; // Центрування вікна при запуску
        }

        // Обробник події для кнопки "Start Game"
        private void btnStartGame_Click(object sender, EventArgs e)
        {
            // Створення екземпляру форми гри та відображення її
            Game gameForm = new Game();
            gameForm.Show();
            this.Hide(); // Приховання поточної форми головного меню
        }

        // Обробник події для кнопки "Best Time"
        private void btnBestTime_Click(object sender, EventArgs e)
        {
            // Зчитування найкращого часу гри
            TimeSpan bestTime = BestTimeManager.ReadBestTime();
            if (bestTime == TimeSpan.MaxValue)
            {
                MessageBox.Show("No best time recorded yet."); // Повідомлення про відсутність записаного найкращого часу
            }
            else
            {
                MessageBox.Show($"Best time is {bestTime.ToString()}"); // Відображення найкращого часу гри
            }
        }

        // Обробник події для кнопки "Quit Game"
        private void btnQuitGame_Click(object sender, EventArgs e)
        {
            Application.Exit(); // Закриття програми
        }
    }
}
