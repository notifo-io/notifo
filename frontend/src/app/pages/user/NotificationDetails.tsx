/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Table } from 'reactstrap';
import { FormatDate } from '@app/framework';
import { UserNotificationChannelDto, UserNotificationDetailsDto } from '@app/service';
import { texts } from '@app/texts';

export interface NotificationDetailsProps {
    // The notification.
    notification: UserNotificationDetailsDto;
}

export const NotificationDetails = (props: NotificationDetailsProps) => {
    const { notification } = props;

    return (
        <div>
            <Table className='user-notification-settings' size='sm'>
                <thead>
                    <tr aria-colspan={5}>
                        <th>{texts.common.settings}</th>
                    </tr>
                </thead>

                <tbody>
                    {Object.entries(notification.channels).map(([key, channel]) =>
                        <tr key={key}>
                            <td>
                                {texts.common.sendModes[channel.setting.send]}
                            </td>
                            <td>
                                {texts.common.via}
                            </td>
                            <td>
                                {texts.notificationSettings[key]?.title}
                            </td>
                            <td>
                                {texts.common.after}
                            </td>
                            <td className='text-right'>
                                {channel.setting.delayInSeconds || 0}
                            </td>
                            <td>
                                {texts.common.secondsShort}
                            </td>
                            <td>
                                {texts.common.when}
                            </td>
                            <td>
                                {channel.setting.condition === 'Inherit' ?  texts.common.conditionModes.Always : texts.common.conditionModes[channel.setting.condition]}
                            </td>
                        </tr>,
                    )}

                </tbody>                                        
            </Table>
            
            <Table className='user-notification-status' size='sm'>
                <thead>
                    <tr>
                        <th>{texts.common.channel}</th>
                        <th>{texts.common.firstDelivered}</th>
                        <th>{texts.common.firstSeen}</th>
                        <th>{texts.common.firstConfirmed}</th>
                        <th>{texts.common.status}</th>
                    </tr>
                </thead>

                <tbody>
                    {Object.entries(notification.channels).map(([key, channel]) =>
                        <tr key={key}>
                            <td>
                                {texts.notificationSettings[key]?.title}
                            </td>
                            <td>
                                <FormatDate format='Ppp' date={channel.firstDelivered} />
                            </td>
                            <td>
                                <FormatDate format='Ppp' date={channel.firstSeen} />
                            </td>
                            <td>
                                <FormatDate format='Ppp' date={channel.firstConfirmed} />
                            </td>
                            <td>
                                {getStatus(channel)}
                            </td>
                        </tr>,
                    )}
                </tbody>
            </Table>
        </div>
    );
};

function getStatus(channel: UserNotificationChannelDto) {
    const status: { [name: string]: boolean } = {};

    for (const value of Object.values(channel.status)) {
        status[value.status || 'None'] = true;
    }

    if (Object.keys(status).length === 0) {
        status['None'] = true;
    }

    return Object.keys(status).sortByString(x => x).join(', ');
}
