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
    // Клас, який представляє вікно паузи гри
    public partial class PauseForm : Form
    {
        // Властивість, що позначає, чи треба продовжувати гру
        public bool ResumeGame { get; set; }

        // Конструктор класу
        public PauseForm()
        {
            InitializeComponent();
        }

        // Обробник події для кнопки "Main Menu"
        private void btnMainMenu_Click(object sender, EventArgs e)
        {
            ResumeGame = false; // Встановлення значення "false", щоб повернутися до головного меню
            this.Close(); // Закриття вікна паузи
        }

        // Обробник події для кнопки "Resume"
        private void btnResume_Click(object sender, EventArgs e)
        {
            ResumeGame = true; // Встановлення значення "true", щоб продовжити гру
            this.Close(); // Закриття вікна паузи
        }
    }
}
