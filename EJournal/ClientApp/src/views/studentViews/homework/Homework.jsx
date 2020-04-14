import React, { useState } from "react";
import * as getListActions from "./reducer";
import { connect } from "react-redux";
import get from "lodash.get";
import {
  Grid,
} from "@material-ui/core";
import InputLabel from "@material-ui/core/InputLabel";
import MenuItem from "@material-ui/core/MenuItem";
import Select from "@material-ui/core/Select";
import "./homeworkStyle.css";
import 'date-fns';
import DateFnsUtils from "@date-io/date-fns";
import deLocale from "date-fns/locale/uk";
import {
  MuiPickersUtilsProvider,
  KeyboardDatePicker,
} from '@material-ui/pickers';
import HomeworkPopover from '../../../components/homework/HomeworkPopover.jsx'
class Homework extends React.Component {
  state = {
    date:null,
    subject: "",
  };
  componentWillReceiveProps = () => {
    const{subject,date} = this.state;
    this.props.getData({subject,date });

  }
  componentDidMount() {
    const { subject } = this.state;
    
    this.props.getData({ subject });
  }
  handleDateChange = (date) => {
      this.setState(
        {
          date: date,
          subject: "",
        });
    }
  render() {
    const { data } = this.props;
    const { subject,date } = this.state;
    let counter=0;
    if (data.subjects != undefined) {
      return (
        <div className="mt-3">
          <div className="d-flex flex-row"> 

          <div className="flex-column">

          
          <InputLabel>Оберіть предмет:</InputLabel>
          <Select
            className="mr-3"
            style={{ minWidth: 150 }}
            value={subject}
            onChange={(e) => {
              this.setState({ subject: e.target.value, date:null });
            }}
            // onChange={handleChange}
          >
            <MenuItem value={""}>Всі</MenuItem>
            {data.subjects.map(function (el) {
              return <MenuItem value={el.id}>{el.name}</MenuItem>;
            })}
          </Select>
          </div>
          <div className="flex-column">
          <InputLabel>Дата:</InputLabel>
          <MuiPickersUtilsProvider utils={DateFnsUtils} locale={deLocale}>
                <KeyboardDatePicker
                  format="dd.MM.yyyy"
                  value={this.state.date}
                  onChange={this.handleDateChange}
                  KeyboardButtonProps={{
                    'aria-label': 'change date',
                  }}
                />
              </MuiPickersUtilsProvider>
              </div>
          </div>
          <Grid className="mt-3" container justify="right" spacing={2}>
            {data.homeworks.map(function (el) {
              counter++;
              return (
                <Grid lg={3} md={4} xl={2} xs={12} item>
                  <HomeworkPopover count={counter} el={el}/>
                </Grid>
              );
            })}
          </Grid>
        </div>
      );
    } else {
      return (<div className='spinner-border text-primary' role='status'>
      <span className='sr-only'>Завантаження...</span>
    </div>);
    }
  }
}
const mapStateToProps = (state) => {
  return {
    errors: get(state, "homework.list.errors"),
    data: get(state, "homework.list.data"),
  };
};

const mapDispatchToProps = (dispatch) => {
  return {
    getData: (filter) => {
      dispatch(getListActions.getData(filter));
    },
  };
};

export default connect(mapStateToProps, mapDispatchToProps)(Homework);