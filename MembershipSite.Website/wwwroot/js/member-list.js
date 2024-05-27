var MemberSite;
(function (MemberSite) {
    var MemberAdmin;
    (function (MemberAdmin) {
        class MemberList {
            static Init() {
                return new MemberList();
            }
            constructor() {
                this.approveAllTicked = document.getElementById("approve-all-ticked");
                this.revokeAllTicked = document.getElementById("revoke-all-ticked");
                this.WireUpUi();
            }
            WireUpUi() {
                // Instantiate the JavaScript grid.
                document.querySelectorAll(".mvc-grid").forEach(element => new MvcGrid(element));
                this.createCheckboxListeners();
            }
            createCheckboxListeners() {
                // Listen for checkbox changes on any row.
                let rowTicks = document.querySelectorAll(".row-check");
                rowTicks
                    .forEach(checkbox => checkbox.addEventListener("change", () => this.rowTicked(rowTicks)));
                // Listen for the 'tick all' button in header.
                let checkAll = document.getElementById("CheckAll");
                checkAll.addEventListener("change", () => {
                    // Select all checkboxes.
                    let checkboxes = document.querySelectorAll("input[type=checkbox].row-check");
                    checkboxes
                        .forEach(checkbox => checkbox.checked = checkAll.checked);
                    // Refesh the buttons that are dependent on the rows being ticked.
                    this.rowTicked(rowTicks);
                });
            }
            rowTicked(rowTicks) {
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
        MemberAdmin.MemberList = MemberList;
    })(MemberAdmin = MemberSite.MemberAdmin || (MemberSite.MemberAdmin = {}));
})(MemberSite || (MemberSite = {}));
//# sourceMappingURL=member-list.js.map