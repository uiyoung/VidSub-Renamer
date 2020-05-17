using System;
using System.Windows.Forms;

namespace VidSubRenamer
{
    class ListViewItemComparer : System.Collections.IComparer
    {
        private int col;
        private string order;

        public ListViewItemComparer()
        {
            col = 0;
        }
        public ListViewItemComparer(int column, string _order)
        {
            col = column;
            order = _order;
        }

        public int Compare(object x, object y)
        {
            if (order.Equals("asc"))
                return String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
            else
                return String.Compare(((ListViewItem)y).SubItems[col].Text, ((ListViewItem)x).SubItems[col].Text);
        }
    }
}
