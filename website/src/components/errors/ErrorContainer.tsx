import React from "react"
import { connect } from "react-redux"
import { ErrorComms } from "./ErrorComms"
import { ErrorFatal } from "./ErrorFatal"
import { ErrorNoMobile } from "./ErrorNoMobile"
import { ErrorNewLogin } from "./ErrorNewLogin"
import { ErrorNetworkMismatch } from "./ErrorNetworkMismatch"
import { ErrorNotInvited } from "./ErrorNotInvited"
import { ErrorNotSupported } from "./ErrorNotSupported"
import { ErrorAvatarLoading } from "./ErrorAvatarLoading"

import "./errors.css"
import { ErrorType, StoreType } from "../../state/redux"

const mapStateToProps = (state: StoreType): ErrorContainerProps => {
  return {
    error: state.error?.type || null,
    details: state.error?.details || null,
  }
}

export interface ErrorContainerProps {
  error: string | null
  details: string | null
}

export const ErrorContainer: React.FC<ErrorContainerProps> = (props) => {
  return (
    <React.Fragment>
      {props.error === ErrorType.FATAL && <ErrorFatal />}
      {props.error === ErrorType.COMMS && <ErrorComms />}
      {props.error === ErrorType.NEW_LOGIN && <ErrorNewLogin />}
      {props.error === ErrorType.NOT_MOBILE && <ErrorNoMobile />}
      {props.error === ErrorType.NOT_INVITED && <ErrorNotInvited />}
      {props.error === ErrorType.NOT_SUPPORTED && <ErrorNotSupported />}
      {props.error === ErrorType.NET_MISMATCH && <ErrorNetworkMismatch details={props.details} />}
      {props.error === ErrorType.AVATAR_ERROR && <ErrorAvatarLoading />}
    </React.Fragment>
  )
}

export default connect(mapStateToProps)(ErrorContainer)
