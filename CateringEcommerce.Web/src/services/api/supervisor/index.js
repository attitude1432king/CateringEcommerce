/**
 * Supervisor API Services
 * Central export for all supervisor-related APIs
 */

import * as supervisorApi from './supervisorApi';
import * as registrationApi from './registrationApi';
import * as assignmentApi from './assignmentApi';
import * as eventSupervisionApi from './eventSupervisionApi';
import * as paymentApi from './paymentApi';
import { getUploadUrl, uploadFile } from './apiConfig';

export {
  supervisorApi,
  registrationApi,
  assignmentApi,
  eventSupervisionApi,
  paymentApi,
  getUploadUrl,
  uploadFile,
};

export default {
  supervisor: supervisorApi,
  registration: registrationApi,
  assignment: assignmentApi,
  eventSupervision: eventSupervisionApi,
  payment: paymentApi,
  upload: {
    getUploadUrl,
    uploadFile,
  },
};
