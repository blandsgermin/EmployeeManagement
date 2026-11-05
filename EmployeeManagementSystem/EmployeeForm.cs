using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmployeeManagementSystem
{
    public partial class EmployeeForm : Form
    {
        DataClasses1DataContext db = new DataClasses1DataContext();

        public EmployeeForm()
        {
            InitializeComponent();
            nudSalary.Maximum = 1000000;
            nudSalary.Minimum = 0;
            dgvEmployees.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvEmployees.MultiSelect = false;
            dgvEmployees.CellClick -= dgvEmployees_CellClick;
            dgvEmployees.CellClick += dgvEmployees_CellClick;
            LoadSex();
            LoadDepartments();
            LoadStatus();
            LoadDepartmentFilter();
            LoadStatusFilter();
            LoadEmployees();
        }

        // Load employees and apply search and filters
        private void LoadEmployees(string search = "")
        {
            try
            {
                // Load from the view so the Age column remains available
                var employees = db.vw_EmployeeDirectories.ToList();

                // Search by Employee ID, First Name, Last Name, or Job Title
                if (!string.IsNullOrWhiteSpace(search))
                {
                    string keyword = search.Trim().ToLower();

                    employees = employees.Where(e =>
                        (e.EmployeeId ?? "").ToLower().Contains(keyword) ||
                        (e.FirstName ?? "").ToLower().Contains(keyword) ||
                        (e.LastName ?? "").ToLower().Contains(keyword) ||
                        (e.JobTitle ?? "").ToLower().Contains(keyword)
                    ).ToList();
                }

                // Filter by department
                if (cboDepartmentFilter.SelectedItem is
                    KeyValuePair<int, string> department &&
                    department.Key != 0)
                {
                    employees = employees
                        .Where(e => e.DepartmentId == department.Key)
                        .ToList();
                }

                // Filter by employment status
                string status = cboStatusFilter.SelectedItem?.ToString();

                if (!string.IsNullOrWhiteSpace(status) &&
                    status != "All Statuses")
                {
                    employees = employees
                        .Where(e => e.EmploymentStatus == status)
                        .ToList();
                }

                // Display the results
                dgvEmployees.DataSource = null;
                dgvEmployees.DataSource = employees;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to load employees: " + ex.Message);
            }
        }

        // Clear all employee fields
        private void ClearForm()
        {
            txtEmployeeId.Clear();
            txtFirstName.Clear();
            txtMiddleName.Clear();
            txtLastName.Clear();

            cboSex.SelectedIndex = -1;
            dtpBirthDate.Value = DateTime.Today;

            txtEmail.Clear();
            txtPhone.Clear();
            txtAddress.Clear();

            cboDepartment.SelectedIndex = -1;
            txtJobTitle.Clear();
            cboStatus.SelectedIndex = -1;

            dtpHireDate.Value = DateTime.Today;
            nudSalary.Value = 0;
        }

        private void txtEmployeeId_TextChanged(object sender, EventArgs e)
        {
        }

        private void txtFirstName_TextChanged(object sender, EventArgs e)
        {
        }

        private void txtMiddleName_TextChanged(object sender, EventArgs e)
        {
        }

        private void txtLastName_TextChanged(object sender, EventArgs e)
        {
        }

        private void cboSex_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void LoadSex()
        {
            cboSex.Items.Clear();
            cboSex.Items.Add("Male");
            cboSex.Items.Add("Female");
        }

        private void dtpBirthDate_ValueChanged(object sender, EventArgs e)
        {
        }

        private void txtEmail_TextChanged(object sender, EventArgs e)
        {
        }

        private void txtPhone_TextChanged(object sender, EventArgs e)
        {
        }

        private void txtAddress_TextChanged(object sender, EventArgs e)
        {
        }

        private void cboDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void LoadDepartments()
        {
            var departments = db.Departments.ToList();

            cboDepartment.DataSource = departments;
            cboDepartment.DisplayMember = "DepartmentName";
            cboDepartment.ValueMember = "DepartmentId";
        }

        private void txtJobTitle_TextChanged(object sender, EventArgs e)
        {
        }

        private void cboStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void LoadStatus()
        {
            cboStatus.Items.Clear();
            cboStatus.Items.Add("Active");
            cboStatus.Items.Add("Inactive");
        }

        private void dtpHireDate_ValueChanged(object sender, EventArgs e)
        {
        }

        private void nudSalary_ValueChanged(object sender, EventArgs e)
        {
        }

        // Update employee
        private void updatebtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmployeeId.Text))
            {
                MessageBox.Show("Please select an employee first.");
                return;
            }

            try
            {
                db.usp_Employee_Update(
                    txtEmployeeId.Text,
                    txtFirstName.Text,
                    string.IsNullOrWhiteSpace(txtMiddleName.Text)
                        ? null : txtMiddleName.Text,
                    txtLastName.Text,
                    cboSex.Text,
                    dtpBirthDate.Value,
                    txtEmail.Text,
                    txtPhone.Text,
                    txtAddress.Text,
                    Convert.ToInt32(cboDepartment.SelectedValue),
                    txtJobTitle.Text,
                    cboStatus.Text,
                    dtpHireDate.Value,
                    nudSalary.Value
                );

                MessageBox.Show("Employee updated successfully!");

                LoadEmployees(txtsearch.Text);
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Create employee
        private void createbtn_Click(object sender, EventArgs e)
        {
            // Check required fields
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(cboSex.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtJobTitle.Text) ||
                string.IsNullOrWhiteSpace(cboStatus.Text) ||
                cboDepartment.SelectedValue == null)
            {
                MessageBox.Show("Please fill in all required fields.");
                return;
            }

            // Check email format
            if (!System.Text.RegularExpressions.Regex.IsMatch(
                txtEmail.Text.Trim(),
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Please enter a valid email address.");
                txtEmail.Focus();
                return;
            }

            // Check birth date and hire date
            if (dtpBirthDate.Value.Date > DateTime.Today)
            {
                MessageBox.Show("Birth date cannot be in the future.");
                return;
            }

            if (dtpHireDate.Value.Date < dtpBirthDate.Value.Date)
            {
                MessageBox.Show(
                    "Hire date cannot be earlier than birth date.");
                return;
            }

            if (nudSalary.Value < 0)
            {
                MessageBox.Show("Salary cannot be negative.");
                return;
            }

            try
            {
                string employeeId = "";

                db.usp_Employee_Create(
                    txtFirstName.Text.Trim(),
                    string.IsNullOrWhiteSpace(txtMiddleName.Text)
                        ? null : txtMiddleName.Text.Trim(),
                    txtLastName.Text.Trim(),
                    cboSex.Text,
                    dtpBirthDate.Value.Date,
                    txtEmail.Text.Trim(),
                    txtPhone.Text.Trim(),
                    txtAddress.Text.Trim(),
                    Convert.ToInt32(cboDepartment.SelectedValue),
                    txtJobTitle.Text.Trim(),
                    cboStatus.Text,
                    dtpHireDate.Value.Date,
                    nudSalary.Value,
                    ref employeeId
                );

                MessageBox.Show(
                    "Employee added successfully!\nEmployee ID: " +
                    employeeId);

                LoadEmployees(txtsearch.Text);
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to add employee.\n" + ex.Message);
            }
        }

        // Delete employee
        private void delbtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmployeeId.Text))
            {
                MessageBox.Show("Please select an employee first.");
                return;
            }

            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this employee?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    db.usp_Employee_Delete(txtEmployeeId.Text);

                    MessageBox.Show("Employee deleted successfully!");

                    LoadEmployees(txtsearch.Text);
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // Search button
        private void searchbtn_Click(object sender, EventArgs e)
        {
            LoadEmployees(txtsearch.Text);
        }

        private void txtsearch_TextChanged(object sender, EventArgs e)
        {
        }

        // Select an employee from the DataGridView
        private void dgvEmployees_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow row = dgvEmployees.Rows[e.RowIndex];

            if (row.IsNewRow)
                return;

            txtEmployeeId.Text =
                Convert.ToString(row.Cells["EmployeeId"].Value);

            txtFirstName.Text =
                Convert.ToString(row.Cells["FirstName"].Value);

            txtMiddleName.Text =
                Convert.ToString(row.Cells["MiddleName"].Value);

            txtLastName.Text =
                Convert.ToString(row.Cells["LastName"].Value);

            cboSex.Text =
                Convert.ToString(row.Cells["Sex"].Value);

            txtEmail.Text =
                Convert.ToString(row.Cells["Email"].Value);

            txtPhone.Text =
                Convert.ToString(row.Cells["Phone"].Value);

            txtAddress.Text =
                Convert.ToString(row.Cells["Address"].Value);

            txtJobTitle.Text =
                Convert.ToString(row.Cells["JobTitle"].Value);

            cboStatus.Text =
                Convert.ToString(row.Cells["EmploymentStatus"].Value);

            if (row.Cells["BirthDate"].Value != null)
                dtpBirthDate.Value =
                    Convert.ToDateTime(row.Cells["BirthDate"].Value);

            if (row.Cells["HireDate"].Value != null)
                dtpHireDate.Value =
                    Convert.ToDateTime(row.Cells["HireDate"].Value);

            if (row.Cells["Salary"].Value != null)
                nudSalary.Value = Math.Min(
                    nudSalary.Maximum,
                    Math.Max(
                        nudSalary.Minimum,
                        Convert.ToDecimal(row.Cells["Salary"].Value)));

            if (row.Cells["DepartmentId"].Value != null)
                cboDepartment.SelectedValue =
                    Convert.ToInt32(row.Cells["DepartmentId"].Value);
        }
        // Open employment history
        private void btnEmploymentHistory_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmployeeId.Text))
            {
                MessageBox.Show("Please select an employee first.");
                return;
            }

            EmploymentHistoryForm form =
                new EmploymentHistoryForm(txtEmployeeId.Text);

            form.ShowDialog();
        }

        private void txtEmployeeId_TextChanged_1(
            object sender, EventArgs e)
        {
        }

        // Department filter event
        private void cboDepartmentFilter_SelectedIndexChanged(
            object sender, EventArgs e)
        {
            LoadEmployees(txtsearch.Text);
        }

        // Load department filter options
        private void LoadDepartmentFilter()
        {
            try
            {
                var departments = db.Departments.ToList();

                cboDepartmentFilter.DataSource = null;
                cboDepartmentFilter.Items.Clear();

                cboDepartmentFilter.Items.Add(
                    new KeyValuePair<int, string>(
                        0, "All Departments"));

                foreach (var department in departments)
                {
                    cboDepartmentFilter.Items.Add(
                        new KeyValuePair<int, string>(
                            department.DepartmentId,
                            department.DepartmentName));
                }

                cboDepartmentFilter.DisplayMember = "Value";
                cboDepartmentFilter.ValueMember = "Key";
                cboDepartmentFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to load departments: " + ex.Message);
            }
        }

        // Status filter event
        private void cboStatusFilter_SelectedIndexChanged(
            object sender, EventArgs e)
        {
            LoadEmployees(txtsearch.Text);
        }

        // Load status filter options
        private void LoadStatusFilter()
        {
            cboStatusFilter.Items.Clear();

            cboStatusFilter.Items.Add("All Statuses");
            cboStatusFilter.Items.Add("Active");
            cboStatusFilter.Items.Add("Resigned");

            cboStatusFilter.SelectedIndex = 0;
        }
    }
}
