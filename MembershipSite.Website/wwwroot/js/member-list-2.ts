namespace MembershipSite.MemberAdmin {

    /*
TODO:
  Have approved as green background?
  Allow inline editing and postback.
  Format date.
  Look at styles used in tabulator samples.
Save data, with save feedback.
Checkbox row selector for selecting multiple rows and running action.
Check paging out with large data set. How does filtering / multi-select tick boxes work with paging?
Option to add data as a row - see https://tabulator.info/docs/6.3/update#alter-add
    Show a BS modal with form to add a new member.
    Delete row needed.
        Warn when deleting current user?
*/

    export class MemberList2 {
        private readonly approveAllTicked = document.getElementById("approve-all-ticked");
        private readonly revokeAllTicked = document.getElementById("revoke-all-ticked");
        private table: any;

        public static Init(): MemberList2 {
            return new MemberList2();
        }

        constructor() {
            this.WireUpUi();
        }

        private WireUpUi(): void {
            this.createGrid();
            //this.createCheckboxListeners();
        }

        private parseField(id: string): number {
            return parseInt((document.getElementById(id) as HTMLInputElement).value);
        }

        private createGrid(): void {

            const fieldLimitMemberNumber = this.parseField("field-limit-member-number");
            const fieldLimitName = this.parseField("field-limit-name");
            const fieldLimitEmail = this.parseField("field-limit-email");

            this.table = new Tabulator("#member-grid", {
                ajaxURL: "/backstage/member-grid-data",
                layout: "fitColumns",
                columns:
                    [
                        { title: "Row", formatter: "rownum", field: "rownum", accessor: "rownum", headerFilter: true },
                        { title: "Member Number", field: "memberNumber", hozAlign: "right", sorter: "number", editor: "input", validator: [`maxLength:${fieldLimitMemberNumber}`, "required"], headerFilter: true },
                        { title: "Name", field: "name", editor: "input", validator: [`maxLength:${fieldLimitName}`, "required"], headerFilter: true },
                        { title: "Email", field: "email", editor: "input", validator: [`maxLength:${fieldLimitEmail}`, "required"], headerFilter: true },
                        {
                            title: "Approved", field: "isApproved", hozAlign: "center", formatter: "toggle",
                            formatterParams: { clickable: true }
                        },
                        {
                            title: "Registered", field: "dateRegistered", headerFilter: true, formatter: "datetime",
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
                            formatter: (cell: any, formatterParams: any) => {
                                return `
                    <button class="btn btn-primary btn-sm action-button" data-id="${cell.getRow().getData().id}">
                        Action
                    </button>
                `;
                            },
                            hozAlign: "center",
                            width: 120,
                        },
                    ],
                persistence: true,
                placeholder: function () {
                    return this.getHeaderFilters().length ? "No Matching Data" : "No Data"; //set placeholder based on if there are currently any header filters
                },
                selectableRows: true,
            });


            //trigger download of data.csv file
            document.getElementById("download-csv").addEventListener("click", () => {
                this.table.download("csv", "membership-report.csv");
            });

            //trigger download of data.json file
            document.getElementById("download-json").addEventListener("click", () => {
                this.table.download("json", "membership-report.json");
            });

            //trigger download of data.xlsx file
            document.getElementById("download-xlsx").addEventListener("click", () => {
                this.table.download("xlsx", "membership-report.xlsx", { sheetName: "Membership report" });
            });

            //trigger download of data.pdf file
            document.getElementById("download-pdf").addEventListener("click", () => {
                this.table.download("pdf", "membership-report.pdf", {
                    orientation: "portrait",
                    title: "Membership database report",
                });
            });

            //trigger download of data.html file
            document.getElementById("download-html").addEventListener("click", () => {
                this.table.download("html", "membership-report.html", { style: true });
            });

            document.getElementById("print-table").addEventListener("click", () => {
                this.table.print(false, true);
            });


            document.getElementById("cancel-button").addEventListener("click", () => {
                this.table.setData();
            });

            document.getElementById("save-button").addEventListener("click", () => {
                // TODO: Track state of data and only enable button when data has changed.
                // Also add POST to save data.
                this.table.getData();
            });

            // Adjust padding on load and resize so that the floating toolbar doesn't cover the last row of the grid.
            window.addEventListener('load', this.adjustGridPadding);
            window.addEventListener('resize', this.adjustGridPadding);

        }

        private adjustGridPadding(): void {
            const toolbar = document.getElementById("bottom-toolbar");
            const grid = document.getElementById('member-grid');

            if (toolbar && grid) {
                const toolbarHeight = toolbar.offsetHeight;
                grid.style.paddingBottom = `${toolbarHeight + 20}px`; // Add some extra space for comfort
            }
        }

        private createCheckboxListeners(): void {
            // Listen for checkbox changes on any row.
            let rowTicks = document.querySelectorAll(".row-check") as NodeListOf<HTMLInputElement>;

            rowTicks
                .forEach(checkbox => checkbox.addEventListener("change", () => this.rowTicked(rowTicks)));

            // Listen for the 'tick all' button in header.
            let checkAll = document.getElementById("CheckAll") as HTMLInputElement;
            checkAll.addEventListener("change", () => {
                // Select all checkboxes.
                let checkboxes = document.querySelectorAll("input[type=checkbox].row-check") as NodeListOf<HTMLInputElement>;

                checkboxes
                    .forEach(checkbox => checkbox.checked = checkAll.checked);

                // Refesh the buttons that are dependent on the rows being ticked.
                this.rowTicked(rowTicks);
            });
        }

        private rowTicked(rowTicks: NodeListOf<HTMLInputElement>): void {
            let approvedCount = 0;
            let unapprovedCount = 0;

            for (const rowTick of rowTicks) {
                if (!rowTick.checked) {
                    continue;
                }

                let parentTr = rowTick.closest("tr");

                if (parentTr.dataset.isApproved.toLowerCase() === "true") {
                    approvedCount++;
                }
                else {
                    unapprovedCount++;
                }
            }

            if (approvedCount) {
                this.approveAllTicked.classList.remove("disabled");
            }
            else {
                this.approveAllTicked.classList.add("disabled");
            }

            if (unapprovedCount) {
                this.revokeAllTicked.classList.remove("disabled");
            }
            else {
                this.revokeAllTicked.classList.add("disabled");
            }
        }
    }
}
