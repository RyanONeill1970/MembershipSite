namespace MembershipSite.MemberAdmin {
    export class MemberList {
        private readonly addMemberForm = document.getElementById("add-member-form") as HTMLFormElement;
        private readonly adminWarningForm = document.getElementById("admin-warning-form") as HTMLFormElement;
        private readonly addMemberModal = document.getElementById("addMemberModal") as HTMLDivElement;
        private readonly adminWarningModal = document.getElementById("adminWarningModal") as HTMLDivElement;
        private readonly grid = document.getElementById("member-grid");
        private table: any;
        private readonly fieldLimitEmail = this.parseField("field-limit-email");
        private readonly fieldLimitMemberNumber = this.parseField("field-limit-membernumber");
        private readonly fieldLimitName = this.parseField("field-limit-name");

        public static Init(): MemberList {
            return new MemberList();
        }

        constructor() {
            this.WireUpUi();
        }

        private WireUpUi(): void {
            this.createGrid();
            this.wireUpAddMemberModal();
            this.wireUpAdminWarningModal();
        }

        private wireUpAddMemberModal(): void {
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

        private wireUpAdminWarningModal(): void {
            this.adminWarningForm.addEventListener("submit", (e) => this.onAdminWarningAcknowledged(e));
        }

        private onAdminWarningAcknowledged(e: SubmitEvent): void {
            e.preventDefault();

            let modalInstance = bootstrap.Modal.getInstance(this.adminWarningModal);
            modalInstance.hide();

            this.saveData();
        }

        private onAddNewMemberFormSubmitted(e: SubmitEvent): void {

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

        private fieldValue(elementId: string, clearValue: boolean): string {
            const element = document.getElementById(elementId) as HTMLInputElement;
            const value = element.value;

            if (clearValue) {
                element.value = "";
            }

            return value;
        }

        private parseField(id: string): number {
            return parseInt((document.getElementById(id) as HTMLInputElement).value);
        }

        private createGrid(): void {

            this.table = new Tabulator(this.grid, {
                ajaxURL: "/backstage/member-grid-data",
                columns:
                    [
                        {
                            editor: "input",
                            title: "Member Number",
                            field: "memberNumber",
                            formatter: (cell: any, formatterParams: any) => this.formatMemberNumberCell(cell, formatterParams),
                            hozAlign: "right",
                            sorter: "number",
                            headerFilter: true,
                            validator: [`maxLength:${this.fieldLimitMemberNumber}`], 
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
                            formatter: (cell: any, formatterParams: any) => this.formatActionCell(cell, formatterParams),
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
                rowFormatter: (row: any) => this.formatRow(row),
                selectableRows: false,
            });

            this.table.on("tableBuilt", () => this.gridReady());
            this.table.on("dataChanged", (data: any) => this.dataChanged(data));
            this.table.on("cellEdited", (cell: any) => this.cellEdited(cell));

            this.wireUpGridDownloadButtons();
            this.wireUpSaveAndCancelButtons();

            window.addEventListener('load', () => this.adjustGridPadding());
            window.addEventListener('resize', () => this.adjustGridPadding());
        }

        private wireUpSaveAndCancelButtons(): void {
            document.getElementById("cancel-button").addEventListener("click", () => {
                this.table.setData();
            });

            document.getElementById("save-button").addEventListener("click", () => this.alertOnAdminChangeAndSave());
        }

        private alertOnAdminChangeAndSave(): void {
            const data = this.table.getData();

            // If any of the rows have been changed from non-admin to admin, show a summary list of new
            // admin users and ask form confirmation.
            const pendingAdminRows = data
                .filter((row: any) => row.pendingAdminChange && !row.isAdmin);

            if (pendingAdminRows.length > 0) {
                const adminWarningList = document.getElementById("adminWarningList") as HTMLUListElement;

                adminWarningList.innerHTML = "";

                pendingAdminRows.forEach((row: any) => {
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

        private async saveData(): Promise<void> {
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
                } else {
                    this.table.alert(`Error saving data. This has been logged. ${this.closeLink()}`);
                    IssueLogger.log("Error saving data", "saveData", { response });
                }
            } catch (error) {
                this.table.alert(`Exception saving data. This has been logged. ${this.closeLink()}`);
                IssueLogger.log("Exception saving data", "saveData", { error });
            }
        }

        private closeLink(): string {
            return "<a href='javascript:window.currentPage.table.clearAlert()'>Close</a>";
        }

        private cellEdited(cell: any): void {
            const rowData = cell.getData();
            const row = this.table.getRow(rowData.memberNumber);

            rowData.isDirty = true;

            this.table.updateData([rowData]);
            this.refreshActionCell(row);
        }

        private dataChanged(data: any): void {
            // If we wanted to set the save / cancel buttons to enabled / disabled based on whether there are changes we
            // could do that here by checking for isDirty on the rows.
        }

        private formatRow(row: any): void {
            const element = row.getElement() as HTMLElement;
            const rowData = row.getData();

            if (rowData.pendingDelete) {
                element.classList.add("table-danger");
            } else if (rowData.isDirty) {
                element.classList.add("table-warning");
            }
        }

        private wireUpGridDownloadButtons(): void {
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

        private gridReady(): void {
            this.grid.addEventListener("click", (e) => this.handleGridClick(e));
            this.enableTooltips();
        }

        private handleGridClick(event: Event) {
            const dropdownButton = (event.target as HTMLElement).closest(".action-button");

            if (dropdownButton) {
                this.handleActionDropdownClick(dropdownButton);
                return;
            }

            const approveMenuItem = (event.target as HTMLElement).closest(".approve-menu-item") as HTMLElement;

            if (approveMenuItem) {
                this.handleApproveMenuItemClick(approveMenuItem);
                return;
            }

            const toggleAdminMenuItem = (event.target as HTMLElement).closest(".toggle-admin-menu-item") as HTMLElement;

            if (toggleAdminMenuItem) {
                this.handleToggleAdminMenuItemClick(toggleAdminMenuItem);
                return;
            }

            const deleteMenuItem = (event.target as HTMLElement).closest(".delete-menu-item") as HTMLElement;

            if (deleteMenuItem) {
                this.handleDeleteMenuItemClick(deleteMenuItem);
            }
        }

        private handleApproveMenuItemClick(approveMenuItem: HTMLElement): void {
            const memberNumber = approveMenuItem.dataset.memberNumber;
            const row = this.table.getRow(memberNumber);
            const rowData = row.getData();

            rowData.isDirty = true;
            rowData.isApproved = true;
            rowData.approveAndSendEmail = true;

            this.table.updateData([rowData]);

            this.closeNearestDropdown(approveMenuItem);
        }

        private handleToggleAdminMenuItemClick(toggleAdminMenuItem: HTMLElement): void {
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

        private refreshActionCell(row: any): void {
            // Manually force the formatter to refresh this cell to update the admin button text.
            const actionsCell = row.getCell("actions");
            actionsCell.setValue(actionsCell.getValue());
        }

        private handleDeleteMenuItemClick(deleteMenuItem: HTMLElement): void {
            const memberNumber = deleteMenuItem.dataset.memberNumber;
            const row = this.table.getRow(memberNumber);
            const rowData = row.getData();

            rowData.isDirty = true;
            rowData.pendingDelete = true;

            this.table.updateData([rowData]);

            this.closeNearestDropdown(deleteMenuItem);
        }

        private closeNearestDropdown(element: Element): void {
            const dropdownMenu = element.closest(".dropdown-menu");
            const dropdownButton = dropdownMenu?.previousElementSibling;
            if (dropdownButton) {
                const dropdownInstance = bootstrap.Dropdown.getInstance(dropdownButton);
                dropdownInstance?.hide();
            }
        }

        private handleActionDropdownClick(button: Element): void {
            const dropdownMenu = button.nextElementSibling as HTMLElement;
            const dropdownInstance = bootstrap.Dropdown.getOrCreateInstance(button,
                {
                    popperConfig: (defaultBsPopperConfig: any) => {
                        // Fix for dropdowns being cut off by the grid container.
                        // See https://github.com/twbs/bootstrap/issues/35774
                        return { ...defaultBsPopperConfig, strategy: 'fixed' };
                    }
                }
            );

            if (dropdownMenu.classList.contains("show")) {
                dropdownInstance.hide();
            } else {
                dropdownInstance.show();
            }

            // Close dropdown on outside click
            const handleOutsideClick = (e: MouseEvent) => {
                if (!dropdownMenu.contains(e.target as Node) && e.target !== button) {
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
        private formatMemberNumberCell(cell: any, formatterParams: any): string {
            const data = cell.getRow().getData();
            let prefix = "";

            if (data.emailLastFailed) {
                const formattedDate = this.formatDate( new Date(data.emailLastFailed));
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
        private formatActionCell(cell: any, formatterParams: any): string {
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

        private adminMenuText(data: any): string {

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
        private adjustGridPadding(): void {
            const toolbar = document.getElementById("bottom-toolbar");

            if (toolbar && this.grid) {
                const toolbarHeight = toolbar.offsetHeight;
                this.grid.style.paddingBottom = `${toolbarHeight + 20}px`; // Add some extra space for comfort
            }
        }

        private formatDate(date: Date): string {
            const dayOfWeek = date.toLocaleString('en', { weekday: 'short' }); // e.g. "Mon"
            const day = this.ordinalSuffix(date.getDate());                    // e.g. "24th"
            const month = date.toLocaleString('en', { month: 'short' });       // e.g. "Jul"
            const year = date.getFullYear();                                   // e.g. 2024

            // Use .padStart(2, '0') to ensure two digits for hours/minutes
            const hours = String(date.getHours()).padStart(2, '0');            // e.g. "14"
            const minutes = String(date.getMinutes()).padStart(2, '0');        // e.g. "56"

            return `${dayOfWeek}, ${day} ${month} ${year} ${hours}:${minutes}`;
        }

        private ordinalSuffix(day: number): string {
            // Special case for teens
            if (day > 3 && day < 21) return day + 'th';
            // Otherwise, look at the last digit
            switch (day % 10) {
                case 1: return day + 'st';
                case 2: return day + 'nd';
                case 3: return day + 'rd';
                default: return day + 'th';
            }
        }

        private enableTooltips(): void {
            const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            tooltipTriggerList.map((tooltipTriggerEl: any) => new bootstrap.Tooltip(tooltipTriggerEl));
        }
    }
}
