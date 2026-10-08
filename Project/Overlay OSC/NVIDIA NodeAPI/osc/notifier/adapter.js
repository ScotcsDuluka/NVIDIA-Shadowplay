(function (window) {
    'use strict';

    if (!window.angular || !window.Notifier) {
        console.error('[notifier] OSC or notifier runtime did not load');
        return;
    }

    angular.module('main').run([
        '$filter',
        '$log',
        'eventAggregator',
        'NOTIFICATION_EVENT',
        'NOTIFICATION_DATA',
        function ($filter, $log, eventAggregator, notificationEvent, notificationData) {
            function findNotification(selection) {
                for (var i = 0; i < notificationData.length; i++) {
                    if (notificationData[i].id === selection) return notificationData[i];
                }
                return null;
            }

            function localizeArgument(value) {
                return typeof value === 'string' && value.indexOf('l10n.') === 0
                    ? $filter('translate')(value)
                    : value;
            }

            function groupFor(selection) {
                if (/^(RECORD_|RECORDING_)/.test(selection)) return 'recording';
                if (/^(INSTANT_REPLAY_|REPLAY_)/.test(selection)) return 'replay';
                if (/^BROADCAST_/.test(selection)) return 'broadcast';
                if (/^SCREENSHOT_|^PHOTOGRAPHIC_SCREENSHOT_/.test(selection)) return 'screenshot';
                return selection.toLowerCase();
            }

            function showNotification(event) {
                if (!event || typeof event.selection !== 'string') {
                    $log.warn('[notifier] Ignored malformed OSC notification', event);
                    return;
                }
                var item = findNotification(event.selection);
                if (!item) {
                    $log.warn('[notifier] No OSC notification definition for ' + event.selection);
                    return;
                }

                var arg1 = localizeArgument(event.arg1);
                var arg2 = localizeArgument(event.arg2);
                var args = { arg1: arg1, arg2: arg2 };
                var message = $filter('translate')(item.message, args);
                var subtext = item.messageSubtext
                    ? $filter('translate')(item.messageSubtext, args)
                    : '';
                window.Notifier.show(message, {
                    key: event.selection,
                    iconClass: item.icon || '',
                    iconPath: item.img || '',
                    positive: /icon-highlighted/.test(item.icon || ''),
                    group: groupFor(event.selection),
                    subtext: subtext
                });
            }

            window.Notifier.setSlotCount(2);
            window.Notifier.startHeartbeat();
            eventAggregator.on(notificationEvent, showNotification);
            window.NvNotifierBridge = {
                show: showNotification,
                status: function () { return window.Notifier.status(); }
            };
            $log.info('[notifier] New stacked notifier connected to OSC events');
        }
    ]);
})(window);
