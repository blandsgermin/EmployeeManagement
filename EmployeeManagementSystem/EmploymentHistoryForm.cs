
using System;
using System.Linq;
using System.Windows.Forms;

namespace EmployeeManagementSystem
{
    public partial class EmploymentHistoryForm : Form
    {
        private readonly DataClasses1DataContext db =
            new DataClasses1DataContext();

        private string employeeId;

        public EmploymentHistoryForm(string employeeId)
        {
            InitializeComponent();

            this.employeeId = employeeId;

            db = new DataClasses1DataContext();

            LoadEmploymentType();
            LoadHistory();
        }

        private void EmploymentHistoryForm_Load(object sender, EventArgs e)
        {
            LoadEmploymentType();
            LoadHistory();
        }


        private void LoadEmploymentType()
        {
            cboEmploymentType.Items.Clear();

            cboEmploymentType.Items.Add("Permanent");
            cboEmploymentType.Items.Add("Contractual");
            cboEmploymentType.Items.Add("Part-time");
            cboEmploymentType.Items.Add("Internship");
            cboEmploymentType.Items.Add("Temporary");
            cboEmploymentType.Items.Add("Volunteer");
            cboEmploymentType.SelectedIndex = -1;
        }
        private void LoadHistory()
        {
            try
            {
                var history = db
                    .usp_EmployeeHistory_ReadByEmployee(employeeId)
                    .ToList();

                dgvHistory.DataSource = null;
                dgvHistory.AutoGenerateColumns = true;
                dgvHistory.DataSource = history;

                dgvHistory.Visible = true;
                dgvHistory.ReadOnly = true;
                dgvHistory.AllowUserToAddRows = false;
                dgvHistory.AllowUserToDeleteRows = false;
                dgvHistory.SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect;
                dgvHistory.MultiSelect = false;
                dgvHistory.AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill;

                if (dgvHistory.Columns["HistoryId"] != null)
                    dgvHistory.Columns["HistoryId"].HeaderText = "History ID";

                if (dgvHistory.Columns["EmployeeId"] != null)
                    dgvHistory.Columns["EmployeeId"].HeaderText = "Employee ID";

                if (dgvHistory.Columns["CompanyName"] != null)
                    dgvHistory.Columns["CompanyName"].HeaderText = "Company";

                if (dgvHistory.Columns["JobTitle"] != null)
                    dgvHistory.Columns["JobTitle"].HeaderText = "Job Title";

                if (dgvHistory.Columns["EmploymentType"] != null)
                    dgvHistory.Columns["EmploymentType"].HeaderText = "Employment Type";

                if (dgvHistory.Columns["StartDate"] != null)
                {
                    dgvHistory.Columns["StartDate"].HeaderText = "Start Date";
                    dgvHistory.Columns["StartDate"].DefaultCellStyle.Format = "MMM dd, yyyy";
                }

                if (dgvHistory.Columns["EndDate"] != null)
                {
                    dgvHistory.Columns["EndDate"].HeaderText = "End Date";
                    dgvHistory.Columns["EndDate"].DefaultCellStyle.Format = "MMM dd, yyyy";
                }

                if (dgvHistory.Columns["ReasonForLeaving"] != null)
                    dgvHistory.Columns["ReasonForLeaving"].HeaderText = "Reason for Leaving";

                if (dgvHistory.Columns["Notes"] != null)
                    dgvHistory.Columns["Notes"].HeaderText = "Notes";

                dgvHistory.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to load employment history.\n" + ex.Message);
            }
        }
        private void chkCurrent_CheckedChanged(object sender, EventArgs e)
        {
            dtpEndDate.Enabled = !chkCurrent.Checked;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCompany.Text) ||
                string.IsNullOrWhiteSpace(txtPreviousTitle.Text))
            {
                MessageBox.Show("Please enter the company and job title.");
                return;
            }

            if (string.IsNullOrWhiteSpace(cboEmploymentType.Text))
            {
                MessageBox.Show("Please select an employment type.");
                return;
            }

            if (!chkCurrent.Checked &&
                dtpEndDate.Value.Date < dtpStartDate.Value.Date)
            {
                MessageBox.Show(
                    "End date cannot be earlier than start date."
                );
                return;
            }

            try
            {
                DateTime? endDate = chkCurrent.Checked
                    ? (DateTime?)null
                    : dtpEndDate.Value.Date;

                db.usp_EmployeeHistory_Create(
                    employeeId,
                    txtCompany.Text.Trim(),
                    txtPreviousTitle.Text.Trim(),
                    cboEmploymentType.Text,
                    dtpStartDate.Value.Date,
                    endDate,
                    txtReason.Text.Trim(),
                    txtNotes.Text.Trim()
                );

                MessageBox.Show("Employment history saved successfully.");

                LoadHistory();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to save record.\n" + ex.Message);
            }
        }
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvHistory.CurrentRow == null)
            {
                MessageBox.Show("Please select a history record.");
                return;
            }

            if (!long.TryParse(
                dgvHistory.CurrentRow.Cells["HistoryId"].Value?.ToString(),
                out long historyId))
            {
                MessageBox.Show("Unable to identify the selected record.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCompany.Text) ||
                string.IsNullOrWhiteSpace(txtPreviousTitle.Text) ||
                string.IsNullOrWhiteSpace(cboEmploymentType.Text))
            {
                MessageBox.Show(
                    "Please complete the company, job title, and employment type.");
                return;
            }

            if (!chkCurrent.Checked &&
                dtpEndDate.Value.Date < dtpStartDate.Value.Date)
            {
                MessageBox.Show(
                    "End date cannot be earlier than start date.");
                return;
            }

            try
            {
                DateTime? endDate = chkCurrent.Checked
                    ? (DateTime?)null
                    : dtpEndDate.Value.Date;

                db.usp_EmployeeHistory_Update(
                    historyId,
                    txtCompany.Text.Trim(),
                    txtPreviousTitle.Text.Trim(),
                    cboEmploymentType.Text,
                    dtpStartDate.Value.Date,
                    endDate,
                    txtReason.Text.Trim(),
                    txtNotes.Text.Trim()
                );

                MessageBox.Show("Employment history updated successfully.");

                LoadHistory();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to update record.\n" + ex.Message);
            }
        }
       private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvHistory.CurrentRow == null)
            {
                MessageBox.Show("Please select a history record.");
                return;
            }

            if (MessageBox.Show(
                "Are you sure you want to delete this record?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            if (!long.TryParse(
                dgvHistory.CurrentRow.Cells["HistoryId"].Value?.ToString(),
                out long historyId))
            {
                MessageBox.Show("Unable to identify the selected record.");
                return;
            }

            try
            {
                db.usp_EmployeeHistory_Delete(historyId);

                MessageBox.Show("Employment history deleted successfully.");

                LoadHistory();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to delete record.\n" + ex.Message);
            }
        }

        private void dgvHistory_CellClick(
            object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow row = dgvHistory.Rows[e.RowIndex];

            if (row.IsNewRow)
                return;

            // Fill the textboxes and ComboBox
            txtCompany.Text =
                row.Cells["CompanyName"].Value?.ToString() ?? "";

            txtPreviousTitle.Text =
                row.Cells["JobTitle"].Value?.ToString() ?? "";

            cboEmploymentType.Text =
                row.Cells["EmploymentType"].Value?.ToString() ?? "";

            // Fill the start date
            if (DateTime.TryParse(
                row.Cells["StartDate"].Value?.ToString(),
                out DateTime startDate))
            {
                dtpStartDate.Value = startDate;
            }

            // Fill the end date and current employment checkbox
            bool hasEndDate = DateTime.TryParse(
                row.Cells["EndDate"].Value?.ToString(),
                out DateTime endDate);

            chkCurrent.Checked = !hasEndDate;
            dtpEndDate.Enabled = hasEndDate;

            if (hasEndDate)
                dtpEndDate.Value = endDate;

            // Fill reason and notes
            txtReason.Text =
                row.Cells["ReasonForLeaving"].Value?.ToString() ?? "";

            txtNotes.Text =
                row.Cells["Notes"].Value?.ToString() ?? "";
        }

        private void ClearForm()
        {
            txtCompany.Clear();
            txtPreviousTitle.Clear();
            cboEmploymentType.SelectedIndex = -1;

            dtpStartDate.Value = DateTime.Today;
            dtpEndDate.Value = DateTime.Today;

            chkCurrent.Checked = false;
            dtpEndDate.Enabled = true;

            txtReason.Clear();
            txtNotes.Clear();

            dgvHistory.ClearSelection();
            dgvHistory.CurrentCell = null;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void txtReason_MultilineChanged(object sender, EventArgs e)
        {
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void label5_Click(object sender, EventArgs e)
        {
        }

        private void dgvHistory_Click(object sender, EventArgs e)
        {
        }

        private void dgvHistory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void txtCompany_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            btnDelete_Click(sender, e);
        }

        private void btnClear_Click_1(object sender, EventArgs e)
        {
            btnClear_Click(sender, e);
        }

        private void btnUpdate_Click_1(object sender, EventArgs e)
        {
            btnUpdate_Click(sender, e);
        }

        private void EmploymentHistoryForm_Load_1(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void txtPreviousTitle_TextChanged(object sender, EventArgs e)
        {

        }

        private void cboEmploymentType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dtpStartDate_ValueChanged(object sender, EventArgs e)
        {

        }

        private void dtpEndDate_ValueChanged(object sender, EventArgs e)
        {

        }

        private void txtReason_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtNotes_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
