import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { Input } from 'reactstrap';
import { Picker } from './Picker';

export default {
    component: Picker,
} as ComponentMeta<typeof Picker>;

const Template = (args: any) => {
    const [value, setValue] = React.useState('');

    return (
        <div id="portals">
            <div className='input-container'>
                <Input value={value} onChange={e => setValue(e.target.value)} />

                <Picker {...args} onPick={e => setValue(x => x + e)} />
            </div>
        </div>
    );
};

export const Default = Template.bind({});

Default['args'] = {
    pickArgument: true,
    pickEmoji: true,
    pickMedia: true,
};
