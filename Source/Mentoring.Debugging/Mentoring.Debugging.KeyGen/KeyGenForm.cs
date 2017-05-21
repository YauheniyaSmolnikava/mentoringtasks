using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace Mentoring.Debugging.KeyGen
{
    public partial class KeyGenForm : Form
    {
        public KeyGenForm()
        {
            InitializeComponent();
        }

        private void btn_generate_Click(object sender, EventArgs e)
        {
            var networkInterface = ((IEnumerable<NetworkInterface>)NetworkInterface.GetAllNetworkInterfaces()).FirstOrDefault<NetworkInterface>();
            byte[] addressBytes = networkInterface.GetPhysicalAddress().GetAddressBytes();
            var dates = BitConverter.GetBytes(DateTime.Now.Date.ToBinary());
            Func<byte, int, int> selector = new Func<byte, int, int>((A_0, A_1) => { return (int)(A_0 ^ dates[A_1]); });
            int[] array = ((IEnumerable<byte>)addressBytes).Select<byte, int>(selector).Select<int, int>((Func<int, int>)(A_0_2 => { return A_0_2 * 10; })).ToArray();
            key.Text = string.Join("-", array);
        }
    }
}
