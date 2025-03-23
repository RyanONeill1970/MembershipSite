var MembershipSite;
(function (MembershipSite) {
    var MemberAdmin;
    (function (MemberAdmin) {
        class Audit {
            static Init() {
                return new Audit();
            }
            constructor() {
                this.grid = document.getElementById("audit-grid");
                this.WireUpUi();
            }
            WireUpUi() {
                this.createGrid();
            }
            createGrid() {
                this.table = new Tabulator(this.grid, {
                    ajaxURL: "/backstage/audit-grid-data",
                    columns: [
                        { title: "Email", field: "email", headerFilter: true },
                        {
                            title: "Date / time", field: "eventOccurred", headerFilter: false, formatter: "datetime",
                            formatterParams: {
                                inputFormat: "iso",
                                outputFormat: "dd/MM/yyyy HH:mm:ss",
                                invalidPlaceholder: "(invalid date)",
                                timezone: "utc",
                            }
                        },
                        { title: "Success", field: "success", headerFilter: true },
                        { title: "Event", field: "eventName", headerFilter: true },
                        {
                            title: "Detail", field: "payload", headerFilter: true, formatter: (cell) => {
                                const value = cell.getValue();
                                const encodedValue = this.encodeDetailField(value);
                                return `<span class="detail-popover" title="${encodedValue}">${value}</span>`;
                            }
                        },
                    ],
                    layout: "fitColumns",
                    persistence: true,
                    placeholder: () => {
                        return "No Data";
                    },
                    selectableRows: false,
                    // Enable pagination
                    pagination: true,
                    paginationMode: "remote", // Use server-side pagination
                    paginationSize: 10, // Number of rows per page
                    paginationSizeSelector: [5, 10, 25, 50, 100], // Page size options
                    paginationInitialPage: 1, // Start on first page
                    ajaxParams: {}, // Additional parameters to send with AJAX requests
                    ajaxConfig: {
                        method: "GET", // Request type
                    },
                    // Add row formatter to highlight rows with success=false
                    rowFormatter: (row) => {
                        const data = row.getData();
                        if (data.success === false) {
                            row.getElement().classList.add("bg-danger", "text-black");
                        }
                    }
                });
                this.table.on("tableBuilt", () => this.gridReady());
                window.addEventListener('load', () => this.adjustGridPadding());
                window.addEventListener('resize', () => this.adjustGridPadding());
            }
            gridReady() {
                this.enableTooltips();
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
            /**
             * If the passed data looks like a JSON string, formats it nicely for display in a popover.
             *
             * Otherwise just returns the input.
             * @param value
             */
            encodeDetailField(value) {
                if (!value)
                    return '';
                try {
                    // Check if the string is valid JSON
                    const obj = JSON.parse(value);
                    // Format the JSON string with indentation for better readability in the tooltip
                    const formattedJson = JSON.stringify(obj, null, 2);
                    // Encode special characters to prevent HTML injection and maintain proper display in tooltip
                    return formattedJson
                        .replace(/&/g, '&amp;')
                        .replace(/</g, '&lt;')
                        .replace(/>/g, '&gt;')
                        .replace(/"/g, '&quot;')
                        .replace(/'/g, '&#039;')
                        .replace(/\n/g, '')
                        .replace(/\s/g, '&nbsp;');
                }
                catch (e) {
                    // If not valid JSON, just encode the string for safe HTML display
                    return value
                        .replace(/&/g, '&amp;')
                        .replace(/</g, '&lt;')
                        .replace(/>/g, '&gt;')
                        .replace(/"/g, '&quot;')
                        .replace(/'/g, '&#039;');
                }
            }
        }
        MemberAdmin.Audit = Audit;
    })(MemberAdmin = MembershipSite.MemberAdmin || (MembershipSite.MemberAdmin = {}));
})(MembershipSite || (MembershipSite = {}));
//# sourceMappingURL=audit.js.map