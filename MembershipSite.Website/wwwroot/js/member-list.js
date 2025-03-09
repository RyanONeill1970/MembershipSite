var MembershipSite;
(function (MembershipSite) {
    var MemberAdmin;
    (function (MemberAdmin) {
        class MemberList {
            static Init() {
                return new MemberList();
            }
            constructor() {
                this.addMemberForm = document.getElementById("add-member-form");
                this.adminWarningForm = document.getElementById("admin-warning-form");
                this.addMemberModal = document.getElementById("addMemberModal");
                this.adminWarningModal = document.getElementById("adminWarningModal");
                this.grid = document.getElementById("member-grid");
                this.fieldLimitEmail = this.parseField("field-limit-email");
                this.fieldLimitName = this.parseField("field-limit-name");
                this.WireUpUi();
            }
            WireUpUi() {
                this.createGrid();
                this.wireUpAddMemberModal();
                this.wireUpAdminWarningModal();
            }
            wireUpAddMemberModal() {
                this.addMemberForm.addEventListener("submit", (e) => this.onAddNewMemberFormSubmitted(e));
                // When showing the modal, focus on the first field.
                this.addMemberModal.addEventListener("shown.bs.modal", () => {
                    document.getElementById("memberName").focus();
                });
                this.addMemberModal.addEventListener("hidden.bs.modal", () => {
                    // Clear any validation errors and the form inputs in case the user shows it again.
                    this.addMemberForm.reset();
                });
            }
            wireUpAdminWarningModal() {
                this.adminWarningForm.addEventListener("submit", (e) => this.onAdminWarningAcknowledged(e));
            }
            onAdminWarningAcknowledged(e) {
                e.preventDefault();
                let modalInstance = bootstrap.Modal.getInstance(this.adminWarningModal);
                modalInstance.hide();
                this.saveData();
            }
            onAddNewMemberFormSubmitted(e) {
                // We'll save the form fields to the table which then saves via a button. No need to let the form submit to the server.
                e.preventDefault();
                const isValid = this.addMemberForm.checkValidity();
                if (!isValid) {
                    return;
                }
                // Fetch the data from the modal.
                const email = this.fieldValue("memberEmail", true);
                const name = this.fieldValue("memberName", true);
                const number = this.fieldValue("memberNumber", true);
                // Add it to tabulator.
                this.table.addData([{ memberNumber: number, name: name, email: email, isDirty: true }], true);
                // Hide the modal.
                let modalInstance = bootstrap.Modal.getInstance(this.addMemberModal);
                modalInstance.hide();
            }
            fieldValue(elementId, clearValue) {
                const element = document.getElementById(elementId);
                const value = element.value;
                if (clearValue) {
                    element.value = "";
                }
                return value;
            }
            parseField(id) {
                return parseInt(document.getElementById(id).value);
            }
            createGrid() {
                this.table = new Tabulator(this.grid, {
                    ajaxURL: "/backstage/member-grid-data",
                    columns: [
                        {
                            title: "Member Number",
                            field: "memberNumber",
                            formatter: (cell, formatterParams) => this.formatMemberNumberCell(cell, formatterParams),
                            hozAlign: "right",
                            sorter: "number",
                            headerFilter: true
                        },
                        { title: "Name", field: "name", editor: "input", validator: [`maxLength:${this.fieldLimitName}`, "required"], headerFilter: true },
                        { title: "Email", field: "email", editor: "input", validator: [`maxLength:${this.fieldLimitEmail}`, "required"], headerFilter: true },
                        {
                            title: "Approved", field: "isApproved", hozAlign: "center", formatter: "toggle",
                            formatterParams: { clickable: true }
                        },
                        {
                            title: "Registered", field: "dateRegistered", headerFilter: false, formatter: "datetime",
                            formatterParams: {
                                inputFormat: "iso",
                                outputFormat: "dd/MM/yyyy HH:mm:ss",
                                invalidPlaceholder: "(invalid date)",
                                timezone: "utc",
                            }
                        },
                        {
                            title: "Actions",
                            field: "actions",
                            formatter: (cell, formatterParams) => this.formatActionCell(cell, formatterParams),
                            headerSort: false,
                            hozAlign: "left",
                        },
                    ],
                    index: "memberNumber",
                    layout: "fitColumns",
                    persistence: true,
                    placeholder: () => {
                        return "No Data";
                    },
                    rowFormatter: (row) => this.formatRow(row),
                    selectableRows: false,
                });
                this.table.on("tableBuilt", () => this.gridReady());
                this.table.on("dataChanged", (data) => this.dataChanged(data));
                this.table.on("cellEdited", (cell) => this.cellEdited(cell));
                this.wireUpGridDownloadButtons();
                this.wireUpSaveAndCancelButtons();
                window.addEventListener('load', () => this.adjustGridPadding());
                window.addEventListener('resize', () => this.adjustGridPadding());
            }
            wireUpSaveAndCancelButtons() {
                document.getElementById("cancel-button").addEventListener("click", () => {
                    this.table.setData();
                });
                document.getElementById("save-button").addEventListener("click", () => this.alertOnAdminChangeAndSave());
            }
            alertOnAdminChangeAndSave() {
                const data = this.table.getData();
                // If any of the rows have been changed from non-admin to admin, show a summary list of new
                // admin users and ask form confirmation.
                const pendingAdminRows = data
                    .filter((row) => row.pendingAdminChange && !row.isAdmin);
                if (pendingAdminRows.length > 0) {
                    const adminWarningList = document.getElementById("adminWarningList");
                    adminWarningList.innerHTML = "";
                    pendingAdminRows.forEach((row) => {
                        const listItem = document.createElement("li");
                        listItem.textContent = `${row.memberNumber} - ${row.name} - ${row.email}`;
                        adminWarningList.appendChild(listItem);
                    });
                    const modalInstance = bootstrap.Modal.getOrCreateInstance(this.adminWarningModal);
                    modalInstance.show();
                    return;
                }
                // No admin promotions, so just save the data.
                this.saveData();
            }
            async saveData() {
                try {
                    this.table.alert("Saving data.");
                    const data = this.table.getData();
                    const response = await fetch("/backstage/save-member-data", {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json",
                        },
                        body: JSON.stringify(data),
                    });
                    if (response.ok) {
                        // Reload data from the server.
                        this.table.setData();
                    }
                    else {
                        this.table.alert(`Error saving data. This has been logged. ${this.closeLink()}`);
                        MemberAdmin.IssueLogger.log("Error saving data", "saveData", { response });
                    }
                }
                catch (error) {
                    this.table.alert(`Exception saving data. This has been logged. ${this.closeLink()}`);
                    MemberAdmin.IssueLogger.log("Exception saving data", "saveData", { error });
                }
            }
            closeLink() {
                return "<a href='javascript:window.currentPage.table.clearAlert()'>Close</a>";
            }
            cellEdited(cell) {
                const rowData = cell.getData();
                const row = this.table.getRow(rowData.memberNumber);
                rowData.isDirty = true;
                this.table.updateData([rowData]);
                this.refreshActionCell(row);
            }
            dataChanged(data) {
                // If we wanted to set the save / cancel buttons to enabled / disabled based on whether there are changes we
                // could do that here by checking for isDirty on the rows.
            }
            formatRow(row) {
                const element = row.getElement();
                const rowData = row.getData();
                if (rowData.pendingDelete) {
                    element.classList.add("table-danger");
                }
                else if (rowData.isDirty) {
                    element.classList.add("table-warning");
                }
            }
            wireUpGridDownloadButtons() {
                // Trigger download of data.csv file.
                document.getElementById("download-csv").addEventListener("click", () => {
                    this.table.download("csv", "membership-report.csv");
                });
                // Trigger download of data.json file.
                document.getElementById("download-json").addEventListener("click", () => {
                    this.table.download("json", "membership-report.json");
                });
                // Trigger download of data.xlsx file.
                document.getElementById("download-xlsx").addEventListener("click", () => {
                    this.table.download("xlsx", "membership-report.xlsx", { sheetName: "Membership report" });
                });
                // Trigger download of data.pdf file.
                document.getElementById("download-pdf").addEventListener("click", () => {
                    this.table.download("pdf", "membership-report.pdf", {
                        orientation: "portrait",
                        title: "Membership database report",
                    });
                });
                // Trigger download of data.html file.
                document.getElementById("download-html").addEventListener("click", () => {
                    this.table.download("html", "membership-report.html", { style: true });
                });
                document.getElementById("print-table").addEventListener("click", () => {
                    this.table.print(false, true);
                });
            }
            gridReady() {
                this.grid.addEventListener("click", (e) => this.handleGridClick(e));
                this.enableTooltips();
            }
            handleGridClick(event) {
                const dropdownButton = event.target.closest(".action-button");
                if (dropdownButton) {
                    this.handleActionDropdownClick(dropdownButton);
                    return;
                }
                const approveMenuItem = event.target.closest(".approve-menu-item");
                if (approveMenuItem) {
                    this.handleApproveMenuItemClick(approveMenuItem);
                    return;
                }
                const toggleAdminMenuItem = event.target.closest(".toggle-admin-menu-item");
                if (toggleAdminMenuItem) {
                    this.handleToggleAdminMenuItemClick(toggleAdminMenuItem);
                    return;
                }
                const deleteMenuItem = event.target.closest(".delete-menu-item");
                if (deleteMenuItem) {
                    this.handleDeleteMenuItemClick(deleteMenuItem);
                }
            }
            handleApproveMenuItemClick(approveMenuItem) {
                const memberNumber = approveMenuItem.dataset.memberNumber;
                const row = this.table.getRow(memberNumber);
                const rowData = row.getData();
                rowData.isDirty = true;
                rowData.isApproved = true;
                rowData.approveAndSendEmail = true;
                this.table.updateData([rowData]);
                this.closeNearestDropdown(approveMenuItem);
            }
            handleToggleAdminMenuItemClick(toggleAdminMenuItem) {
                const memberNumber = toggleAdminMenuItem.dataset.memberNumber;
                const row = this.table.getRow(memberNumber);
                const rowData = row.getData();
                rowData.isDirty = true;
                // Tell the server to toggle the current status of isAdmin.
                rowData.pendingAdminChange = !rowData.pendingAdminChange;
                this.table.updateData([rowData]);
                this.refreshActionCell(row);
                this.closeNearestDropdown(toggleAdminMenuItem);
            }
            refreshActionCell(row) {
                // Manually force the formatter to refresh this cell to update the admin button text.
                const actionsCell = row.getCell("actions");
                actionsCell.setValue(actionsCell.getValue());
            }
            handleDeleteMenuItemClick(deleteMenuItem) {
                const memberNumber = deleteMenuItem.dataset.memberNumber;
                const row = this.table.getRow(memberNumber);
                const rowData = row.getData();
                rowData.isDirty = true;
                rowData.pendingDelete = true;
                this.table.updateData([rowData]);
                this.closeNearestDropdown(deleteMenuItem);
            }
            closeNearestDropdown(element) {
                const dropdownMenu = element.closest(".dropdown-menu");
                const dropdownButton = dropdownMenu === null || dropdownMenu === void 0 ? void 0 : dropdownMenu.previousElementSibling;
                if (dropdownButton) {
                    const dropdownInstance = bootstrap.Dropdown.getInstance(dropdownButton);
                    dropdownInstance === null || dropdownInstance === void 0 ? void 0 : dropdownInstance.hide();
                }
            }
            handleActionDropdownClick(button) {
                const dropdownMenu = button.nextElementSibling;
                const dropdownInstance = bootstrap.Dropdown.getOrCreateInstance(button, {
                    popperConfig: (defaultBsPopperConfig) => {
                        // Fix for dropdowns being cut off by the grid container.
                        // See https://github.com/twbs/bootstrap/issues/35774
                        return Object.assign(Object.assign({}, defaultBsPopperConfig), { strategy: 'fixed' });
                    }
                });
                if (dropdownMenu.classList.contains("show")) {
                    dropdownInstance.hide();
                }
                else {
                    dropdownInstance.show();
                }
                // Close dropdown on outside click
                const handleOutsideClick = (e) => {
                    if (!dropdownMenu.contains(e.target) && e.target !== button) {
                        dropdownInstance.hide();
                        document.removeEventListener("click", handleOutsideClick);
                    }
                };
                document.addEventListener("click", handleOutsideClick);
            }
            /**
             * Formats the cell for the member number to include a warning if the member could not be emailed.
             * Also shows an info icon if the member has been successfully emailed.
             * @param cell
             * @param formatterParams
             */
            formatMemberNumberCell(cell, formatterParams) {
                const data = cell.getRow().getData();
                let prefix = "";
                if (data.emailLastFailed) {
                    const formattedDate = this.formatDate(new Date(data.emailLastFailed));
                    const title = `An email was sent on '${formattedDate}' to this member but failed to be delivered. See the audit log for more details.`;
                    prefix = `<i class="bi bi-exclamation-triangle text-danger" title="${title}" data-bs-toggle="tooltip" data-bs-title="${title}"></i> `;
                }
                else if (data.emailLastSucceeded) {
                    const formattedDate = this.formatDate(new Date(data.emailLastSucceeded));
                    const title = `An email was successfully delivered on '${formattedDate}' to this members service provider.`;
                    prefix = `<i class="bi bi-info-circle text-info" title="${title}" data-bs-toggle="tooltip" data-bs-title="${title}"></i> `;
                }
                return `${prefix}${data.memberNumber}`;
            }
            /**
             * Formats the action cell for a row. The cell can be inspected to read the row data and render
             * a split button with actions for "Delete" and "Approve and send email".
             *
             * For example:
             *
             *  const data = cell.getRow().getData();
             *
             *  if (data.isApproved) {
             *      // Approve button will be output as disabled.
             *  }
             *
             * @param cell
             * @param formatterParams
             * @returns {string}
             */
            formatActionCell(cell, formatterParams) {
                const data = cell.getRow().getData();
                let isAdminButtonText = this.adminMenuText(data);
                return `
                <div class="btn-group">
                    <button 
                        type="button" 
                        class="btn btn-primary dropdown-toggle action-button" 
                        aria-expanded="false">
                        <i class="bi bi-file-earmark"></i> Actions
                    </button>
                    <div class="dropdown-menu p-2">
                        <button
                            class="dropdown-item approve-menu-item" 
                            data-member-number="${data.memberNumber}" 
                            ${data.isApproved ? 'disabled' : ''}>
                            Approve and send email
                        </button>
                        <button class="dropdown-item toggle-admin-menu-item" data-member-number="${data.memberNumber}">
                            ${isAdminButtonText}
                        </button>
                        <button class="dropdown-item delete-menu-item" data-member-number="${data.memberNumber}">
                            Delete
                        </button>
                    </div>
                </div>
                `;
            }
            adminMenuText(data) {
                if (data.isAdmin) {
                    if (data.pendingAdminChange) {
                        return "Pending admin removal";
                    }
                    else {
                        return "Remove admin";
                    }
                }
                if (data.pendingAdminChange) {
                    return "Pending admin promotion";
                }
                return "Make admin";
            }
            /**
             * Adjusts the padding of the grid to accommodate the toolbar at the bottom of the page so that the grid is not obscured by the buttons.
             */
            adjustGridPadding() {
                const toolbar = document.getElementById("bottom-toolbar");
                if (toolbar && this.grid) {
                    const toolbarHeight = toolbar.offsetHeight;
                    this.grid.style.paddingBottom = `${toolbarHeight + 20}px`; // Add some extra space for comfort
                }
            }
            formatDate(date) {
                const dayOfWeek = date.toLocaleString('en', { weekday: 'short' }); // e.g. "Mon"
                const day = this.ordinalSuffix(date.getDate()); // e.g. "24th"
                const month = date.toLocaleString('en', { month: 'short' }); // e.g. "Jul"
                const year = date.getFullYear(); // e.g. 2024
                // Use .padStart(2, '0') to ensure two digits for hours/minutes
                const hours = String(date.getHours()).padStart(2, '0'); // e.g. "14"
                const minutes = String(date.getMinutes()).padStart(2, '0'); // e.g. "56"
                return `${dayOfWeek}, ${day} ${month} ${year} ${hours}:${minutes}`;
            }
            ordinalSuffix(day) {
                // Special case for teens
                if (day > 3 && day < 21)
                    return day + 'th';
                // Otherwise, look at the last digit
                switch (day % 10) {
                    case 1: return day + 'st';
                    case 2: return day + 'nd';
                    case 3: return day + 'rd';
                    default: return day + 'th';
                }
            }
            enableTooltips() {
                const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
                tooltipTriggerList.map((tooltipTriggerEl) => new bootstrap.Tooltip(tooltipTriggerEl));
            }
        }
        MemberAdmin.MemberList = MemberList;
    })(MemberAdmin = MembershipSite.MemberAdmin || (MembershipSite.MemberAdmin = {}));
})(MembershipSite || (MembershipSite = {}));
//# sourceMappingURL=member-list.js.map