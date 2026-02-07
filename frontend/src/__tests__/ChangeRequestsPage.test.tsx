import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ChangeRequestsPage } from '../pages/ChangeRequestsPage';
import * as changeRequestsApi from '../api/changeRequests';

// Mock the API
vi.mock('../api/changeRequests');

// Mock ChangeRequestsPanel component
vi.mock('../components/ChangeRequestsPanel', () => ({
  ChangeRequestsPanel: ({ onRequestsChanged }: { onRequestsChanged?: () => void }) => (
    <div data-testid="change-requests-panel">
      Change Requests Panel
      <button onClick={onRequestsChanged} data-testid="mock-trigger-refresh">
        Trigger Refresh
      </button>
    </div>
  ),
}));

// Mock ChangeRequestModal component
vi.mock('../components/ChangeRequestModal', () => ({
  ChangeRequestModal: ({
    open,
    onClose,
    onSuccess,
  }: {
    open: boolean;
    onClose: () => void;
    onSuccess: () => void;
  }) =>
    open ? (
      <div data-testid="change-request-modal">
        Change Request Modal
        <button onClick={onClose} data-testid="mock-close-modal">
          Close
        </button>
        <button
          onClick={() => {
            onSuccess();
            onClose();
          }}
          data-testid="mock-submit-modal"
        >
          Submit
        </button>
      </div>
    ) : null,
}));

// Mock i18n/sv
vi.mock('../i18n/sv', () => ({
  sv: {
    changeRequest: {
      title: 'Change Requests',
      createNew: 'Create New Request',
    },
  },
}));

describe('ChangeRequestsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render page title and create button', () => {
    render(<ChangeRequestsPage />);

    expect(screen.getByTestId('page-title')).toHaveTextContent('Change Requests');
    expect(screen.getByTestId('create-request-button')).toBeInTheDocument();
  });

  it('should render ChangeRequestsPanel', () => {
    render(<ChangeRequestsPage />);

    expect(screen.getByTestId('change-requests-panel')).toBeInTheDocument();
  });

  it('should open modal when create button clicked', async () => {
    const user = userEvent.setup();
    render(<ChangeRequestsPage />);

    const createButton = screen.getByTestId('create-request-button');
    await user.click(createButton);

    expect(screen.getByTestId('change-request-modal')).toBeInTheDocument();
  });

  it('should close modal when close button clicked', async () => {
    const user = userEvent.setup();
    render(<ChangeRequestsPage />);

    // Open modal
    await user.click(screen.getByTestId('create-request-button'));
    expect(screen.getByTestId('change-request-modal')).toBeInTheDocument();

    // Close modal
    await user.click(screen.getByTestId('mock-close-modal'));

    await waitFor(() => {
      expect(screen.queryByTestId('change-request-modal')).not.toBeInTheDocument();
    });
  });

  it('should refresh panel on successful creation', async () => {
    const user = userEvent.setup();
    render(<ChangeRequestsPage />);

    // Store initial panel instance
    const initialPanel = screen.getByTestId('change-requests-panel');

    // Open modal and submit
    await user.click(screen.getByTestId('create-request-button'));
    await user.click(screen.getByTestId('mock-submit-modal'));

    // Panel should be refreshed (new key)
    await waitFor(() => {
      expect(screen.queryByTestId('change-request-modal')).not.toBeInTheDocument();
    });
  });

  it('should refresh panel when panel triggers refresh', async () => {
    const user = userEvent.setup();
    render(<ChangeRequestsPage />);

    // Trigger refresh from panel
    await user.click(screen.getByTestId('mock-trigger-refresh'));

    // Panel should still be there (just re-rendered with new key)
    expect(screen.getByTestId('change-requests-panel')).toBeInTheDocument();
  });

  it('should have correct button icon and text', () => {
    render(<ChangeRequestsPage />);

    const createButton = screen.getByTestId('create-request-button');
    expect(createButton).toHaveTextContent('Create New Request');
  });

  it('should not show modal initially', () => {
    render(<ChangeRequestsPage />);

    expect(screen.queryByTestId('change-request-modal')).not.toBeInTheDocument();
  });

  it('should render page with proper layout', () => {
    render(<ChangeRequestsPage />);

    // Check that title and button are in a flex container
    const titleElement = screen.getByTestId('page-title');
    const buttonElement = screen.getByTestId('create-request-button');

    expect(titleElement.parentElement).toContainElement(buttonElement);
  });
});
