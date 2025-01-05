namespace MembershipSite.MemberAdmin {

    /*
TODO:
    Save data, with save feedback.
        Needs to show overlay with spinner while saving.
    Option to add data as a row - see https://tabulator.info/docs/6.3/update#alter-add
    Show a BS modal with form to add a new member.
    Delete row needed.
*/

    export class MemberList2 {
        private readonly grid = document.getElementById("member-grid");
        private table: any;

        public static Init(): MemberList2 {
            return new MemberList2();
        }

        constructor() {
            this.WireUpUi();
        }

        private WireUpUi(): void {
            this.createGrid();
        }

        private parseField(id: string): number {
            return parseInt((document.getElementById(id) as HTMLInputElement).value);
        }

        private createGrid(): void {

            const fieldLimitMemberNumber = this.parseField("field-limit-member-number");
            const fieldLimitName = this.parseField("field-limit-name");
            const fieldLimitEmail = this.parseField("field-limit-email");

            this.table = new Tabulator(this.grid, {
                ajaxURL: "/backstage/member-grid-data",
                columns:
                    [
                        { title: "Member Number", field: "memberNumber", hozAlign: "right", sorter: "number", editor: "input", validator: [`maxLength:${fieldLimitMemberNumber}`, "required"], headerFilter: true },
                        { title: "Name", field: "name", editor: "input", validator: [`maxLength:${fieldLimitName}`, "required"], headerFilter: true },
                        { title: "Email", field: "email", editor: "input", validator: [`maxLength:${fieldLimitEmail}`, "required"], headerFilter: true },
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
                            formatter: this.formatActionCell,
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

        private wireUpSaveAndCancelButtons() :void {
            document.getElementById("cancel-button").addEventListener("click", () => {
                this.table.setData();
            });

            document.getElementById("save-button").addEventListener("click", () => {
                // TODO: Track state of data and only enable button when data has changed.
                // Also add POST to save data.
                this.table.getData();

                this.table.alert("Saving data.");
            });
        }

        private cellEdited(cell: any): void {
            const rowData = cell.getData();

            rowData.isDirty = true;
            rowData.approveAndSendEmail = true;

            this.table.updateData([rowData]);
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
            rowData.approveAndSendEmail = true;

            this.table.updateData([rowData]);

            this.closeNearestDropdown(approveMenuItem);
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
                            ${data.isApproved ? 'disabled' : ''}
                        >
                            Approve and send email
                        </button>
                        <button class="dropdown-item delete-menu-item" data-member-number="${data.memberNumber}">
                            Delete
                        </button>
                    </div>
                </div>
                `;
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
    }
}
