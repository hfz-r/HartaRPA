// import { actions as CommonActions } from '../reducers/app';

export default (dispatch: any) => {
  window.addEventListener('online', updateOnlineStatus);
  window.addEventListener('offline', updateOnlineStatus);

  function updateOnlineStatus(event: any) {
    console.log('Online status: ' + event.type);
    if (navigator.onLine) {
      //dispatch(CommonActions.goOnline());
    } else {
      //dispatch(CommonActions.goOffline());
    }
  }
};
