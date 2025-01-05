namespace MembershipSite.MemberAdmin {
    export class IssueLogger {
        public static log(message: string, tag?: string, context?: any): void {
            try {
                if (!Sentry) {
                    return;
                }

                if (context) {
                    Sentry.configureScope((scope: any) => {
                        scope.setExtra("additional", context);
                    });
                }

                if (tag) {
                    Sentry.configureScope((scope: any) => {
                        scope.setTag("operation", tag);
                    });
                }

                Sentry.captureMessage(message);
            } catch (e) {
                // Sentry not loaded, probably due to ad blockers.
            }
        }
    }
}
