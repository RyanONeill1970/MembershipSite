﻿namespace MembershipSite.MemberAdmin {
    export class Audit {
        private readonly grid = document.getElementById("audit-grid");
        private table: any;

        public static Init(): Audit {
            return new Audit();
        }

        constructor() {
            this.WireUpUi();
        }

        private WireUpUi(): void {
            this.createGrid();
        }

        private createGrid(): void {

            this.table = new Tabulator(this.grid, {
                ajaxURL: "/backstage/audit-grid-data",
                columns:
                    [
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
                        { title: "Detail", field: "payload", headerFilter: true },
                    ],
                layout: "fitColumns",
                persistence: true,
                placeholder: () => {
                    return "No Data";
                },
                selectableRows: false,
            });

            window.addEventListener('load', () => this.adjustGridPadding());
            window.addEventListener('resize', () => this.adjustGridPadding());
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
    }
}
