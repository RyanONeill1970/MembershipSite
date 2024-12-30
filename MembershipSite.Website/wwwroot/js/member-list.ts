namespace MembershipSite.MemberAdmin {

    // TODO: Use package manager to download MVCGrid and get typedefs.
    declare let MvcGrid: any;

    export class MemberList {
        private readonly approveAllTicked = document.getElementById("approve-all-ticked");
        private readonly revokeAllTicked = document.getElementById("revoke-all-ticked");

        public static Init(): MemberList {
            return new MemberList();
        }

        constructor() {
            this.WireUpUi();
        }

        private WireUpUi(): void {
            // Instantiate the JavaScript grid.
            document.querySelectorAll(".mvc-grid").forEach(element => new MvcGrid(element));

            this.createCheckboxListeners();
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
