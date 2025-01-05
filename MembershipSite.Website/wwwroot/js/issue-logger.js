var MembershipSite;
(function (MembershipSite) {
    var MemberAdmin;
    (function (MemberAdmin) {
        class IssueLogger {
            static log(message, tag, context) {
                try {
                    if (!Sentry) {
                        return;
                    }
                    if (context) {
                        Sentry.configureScope((scope) => {
                            scope.setExtra("additional", context);
                        });
                    }
                    if (tag) {
                        Sentry.configureScope((scope) => {
                            scope.setTag("operation", tag);
                        });
                    }
                    Sentry.captureMessage(message);
                }
                catch (e) {
                    // Sentry not loaded, probably due to ad blockers.
                }
            }
        }
        MemberAdmin.IssueLogger = IssueLogger;
    })(MemberAdmin = MembershipSite.MemberAdmin || (MembershipSite.MemberAdmin = {}));
})(MembershipSite || (MembershipSite = {}));
//# sourceMappingURL=issue-logger.js.map