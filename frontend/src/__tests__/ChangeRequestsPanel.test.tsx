import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ChangeRequestsPanel } from '../components/ChangeRequestsPanel';
import * as changeRequestsApi from '../api/changeRequests';
import type { ChangeRequestDto } from '../api/changeRequests';

const { mockAuthHeader } = vi.hoisted(() => ({
  mockAuthHeader: vi.fn(() => ({ Authorization: 'Bearer test-token' })),
}));

// Mock the API
vi.mock('../api/changeRequests');

// Mock AuthContext
vi.mock('../auth/AuthContext', () => ({
  useAuth: () => ({
    user: {
      email: 'parenta@test.com',
      role: 'ParentA',
      displayName: 'Parent A',
    },
    authHeader: mockAuthHeader,
  }),
}));

// Mock i18n/sv
vi.mock('../i18n/sv', () => ({
  sv: {
    changeRequest: {
      tabs: {
        pending: 'Pending',
        all: 'All',
      },
      noRequests: 'No change requests',
      requestedBy: 'Requested by',
      comment: 'Comment',
      reviewedBy: 'Reviewed by',
      review: 'Review',
      confirmCancel: 'Are you sure?',
      reviewDialog: {
        title: 'Review Change Request',
        date: 'Date',
        requestedBy: 'Requested by',
        responseComment: 'Your response (optional)',
        approve: 'Approve',
        reject: 'Reject',
      },
    },
    common: {
      cancel: 'Cancel',
    },
  },
}));

describe('ChangeRequestsPanel', () => {
  const mockRequest: ChangeRequestDto = {
    id: 1,
    requestedByName: 'Parent B',
    requestedForDate: '2025-03-15',
    currentParent: 'A',
    requestedParent: 'B',
    status: 'Pending',
    comment: 'Need to switch',
    createdAt: '2025-03-10T10:00:00Z',
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render pending tab by default', async () => {
    vi.spyOn(changeRequestsApi, 'getPendingChangeRequests').mockResolvedValue([]);

    render(<ChangeRequestsPanel />);

    expect(screen.getByTestId('pending-tab')).toHaveAttribute('aria-selected', 'true');
    await waitFor(() => {
      expect(screen.getByTestId('no-requests')).toBeInTheDocument();
    });
  });

  it('should switch to all tab', async () => {
    const user = userEvent.setup();
    vi.spyOn(changeRequestsApi, 'getPendingChangeRequests').mockResolvedValue([]);
    vi.spyOn(changeRequestsApi, 'getMyChangeRequests').mockResolvedValue([]);

    render(<ChangeRequestsPanel />);

    await user.click(screen.getByTestId('all-tab'));

    expect(screen.getByTestId('all-tab')).toHaveAttribute('aria-selected', 'true');
  });

  it('should display list of requests', async () => {
    vi.spyOn(changeRequestsApi, 'getPendingChangeRequests').mockResolvedValue([
      mockRequest,
    ]);

    render(<ChangeRequestsPanel />);

    await waitFor(() => {
      expect(screen.getByTestId('request-1')).toBeInTheDocument();
      expect(screen.getByText('2025-03-15')).toBeInTheDocument();
      expect(screen.getByText(/Parent B/)).toBeInTheDocument();
      expect(screen.getByTestId('status-1')).toHaveTextContent('Pending');
    });
  });

  it('should show review button for requests affecting user', async () => {
    vi.spyOn(changeRequestsApi, 'getPendingChangeRequests').mockResolvedValue([
      mockRequest, // CurrentParent = "A", user is ParentA
    ]);

    render(<ChangeRequestsPanel />);

    await waitFor(() => {
      expect(screen.getByTestId('review-button-1')).toBeInTheDocument();
    });
  });

  it('should show cancel button for own requests', async () => {
    const ownRequest: ChangeRequestDto = {
      ...mockRequest,
      requestedByName: 'Parent A', // Same as logged in user
      currentParent: 'B',
      requestedParent: 'A',
    };

    vi.spyOn(changeRequestsApi, 'getPendingChangeRequests').mockResolvedValue([
      ownRequest,
    ]);

    render(<ChangeRequestsPanel />);

    await waitFor(() => {
      expect(screen.getByTestId('cancel-button-1')).toBeInTheDocument();
    });
  });

  it('should not show review button for own requests', async () => {
    const ownRequest: ChangeRequestDto = {
      ...mockRequest,
      requestedByName: 'Parent A',
      currentParent: 'B',
      requestedParent: 'A',
    };

    vi.spyOn(changeRequestsApi, 'getPendingChangeRequests').mockResolvedValue([
      ownRequest,
    ]);

    render(<ChangeRequestsPanel />);

    await waitFor(() => {
      expect(screen.queryByTestId('review-button-1')).not.toBeInTheDocument();
    });
  });

  it('should open review dialog when review button clicked', async () => {
    const user = userEvent.setup();
    vi.spyOn(changeRequestsApi, 'getPendingChangeRequests').mockResolvedValue([
      mockRequest,
    ]);

    render(<ChangeRequestsPanel />);

    await waitFor(() => {
      expect(screen.getByTestId('review-button-1')).toBeInTheDocument();
    });

    await user.click(screen.getByTestId('review-button-1'));

    expect(screen.getByText('Review Change Request')).toBeInTheDocument();
    expect(screen.getByTestId('approve-button')).toBeInTheDocument();
    expect(screen.getByTestId('reject-button')).toBeInTheDocument();
  });

  it('should approve request', async () => {
    const user = userEvent.setup();
    const mockReview = vi
      .spyOn(changeRequestsApi, 'reviewChangeRequest')
      .mockResolvedValue({ ...mockRequest, status: 'Approved' });
    vi.spyOn(changeRequestsApi, 'getPendingChangeRequests').mockResolvedValue([
      mockRequest,
    ]);

    render(<ChangeRequestsPanel />);

    await waitFor(() => {
      expect(screen.getByTestId('review-button-1')).toBeInTheDocument();
    });

    await user.click(screen.getByTestId('review-button-1'));
    await user.click(screen.getByTestId('approve-button'));

    await waitFor(() => {
      expect(mockReview).toHaveBeenCalledWith(1, true, expect.any(Function), undefined);
    });
  });

  it('should reject request with comment', async () => {
    const user = userEvent.setup();
    const mockReview = vi
      .spyOn(changeRequestsApi, 'reviewChangeRequest')
      .mockResolvedValue({ ...mockRequest, status: 'Rejected' });
    vi.spyOn(changeRequestsApi, 'getPendingChangeRequests').mockResolvedValue([
      mockRequest,
    ]);

    render(<ChangeRequestsPanel />);

    await waitFor(() => {
      expect(screen.getByTestId('review-button-1')).toBeInTheDocument();
    });

    await user.click(screen.getByTestId('review-button-1'));

    const commentInput = screen.getByTestId('review-comment-input').querySelector('textarea');
    if (commentInput) {
      await user. type(commentInput, 'Sorry, cannot switch');
    }

    await user.click(screen.getByTestId('reject-button'));

    await waitFor(() => {
      expect(mockReview).toHaveBeenCalledWith(1, false, expect.any(Function), 'Sorry, cannot switch');
    });
  });

  it('should cancel request after confirmation', async () => {
    const user = userEvent.setup();
    const mockCancel = vi
      .spyOn(changeRequestsApi, 'cancelChangeRequest')
      .mockResolvedValue();

    const ownRequest: ChangeRequestDto = {
      ...mockRequest,
      requestedByName: 'Parent A',
      currentParent: 'B',
      requestedParent: 'A',
    };

    vi.spyOn(changeRequestsApi, 'getPendingChangeRequests').mockResolvedValue([
      ownRequest,
    ]);

    // Mock window.confirm
    window.confirm = vi.fn(() => true);

    render(<ChangeRequestsPanel />);

    await waitFor(() => {
      expect(screen.getByTestId('cancel-button-1')).toBeInTheDocument();
    });

    await user.click(screen.getByTestId('cancel-button-1'));

    await waitFor(() => {
      expect(mockCancel).toHaveBeenCalledWith(1, expect.any(Function));
    });
  });

  it('should not cancel request if not confirmed', async () => {
    const user = userEvent.setup();
    const mockCancel = vi
      .spyOn(changeRequestsApi, 'cancelChangeRequest')
      .mockResolvedValue();

    const ownRequest: ChangeRequestDto = {
      ...mockRequest,
      requestedByName: 'Parent A',
      currentParent: 'B',
      requestedParent: 'A',
    };

    vi.spyOn(changeRequestsApi, 'getPendingChangeRequests').mockResolvedValue([
      ownRequest,
    ]);

    // Mock window.confirm to return false
    window.confirm = vi.fn(() => false);

    render(<ChangeRequestsPanel />);

    await waitFor(() => {
      expect(screen.getByTestId('cancel-button-1')).toBeInTheDocument();
    });

    await user.click(screen.getByTestId('cancel-button-1'));

    expect(mockCancel).not.toHaveBeenCalled();
  });

  it('should display status badges with correct colors', async () => {
    const requests: ChangeRequestDto[] = [
      { ...mockRequest, id: 1, status: 'Pending' },
      { ...mockRequest, id: 2, status: 'Approved', requestedForDate: '2025-03-16' },
      { ...mockRequest, id: 3, status: 'Rejected', requestedForDate: '2025-03-17' },
      { ...mockRequest, id: 4, status: 'Cancelled', requestedForDate: '2025-03-18' },
    ];

    vi.spyOn(changeRequestsApi, 'getMyChangeRequests').mockResolvedValue(requests);

    const user = userEvent.setup();
    render(<ChangeRequestsPanel />);

    await user.click(screen.getByTestId('all-tab'));

    await waitFor(() => {
      expect(screen.getByTestId('status-1')).toHaveTextContent('Pending');
      expect(screen.getByTestId('status-2')).toHaveTextContent('Approved');
      expect(screen.getByTestId('status-3')).toHaveTextContent('Rejected');
      expect(screen.getByTestId('status-4')).toHaveTextContent('Cancelled');
    });
  });

  it('should call onRequestsChanged callback', async () => {
    const mockCallback = vi.fn();
    const user = userEvent.setup();

    const ownRequest: ChangeRequestDto = {
      ...mockRequest,
      requestedByName: 'Parent A',
      currentParent: 'B',
      requestedParent: 'A',
    };

    vi.spyOn(changeRequestsApi, 'getPendingChangeRequests').mockResolvedValue([
      ownRequest,
    ]);
    vi.spyOn(changeRequestsApi, 'cancelChangeRequest').mockResolvedValue();

    window.confirm = vi.fn(() => true);

    render(<ChangeRequestsPanel onRequestsChanged={mockCallback} />);

    await waitFor(() => {
      expect(screen.getByTestId('cancel-button-1')).toBeInTheDocument();
    });

    await user.click(screen.getByTestId('cancel-button-1'));

    await waitFor(() => {
      expect(mockCallback).toHaveBeenCalled();
    });
  });

  it('should show loading spinner while loading', () => {
    vi.spyOn(changeRequestsApi, 'getPendingChangeRequests').mockImplementation(
      () => new Promise(() => {}) // Never resolves
    );

    render(<ChangeRequestsPanel />);

    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });
});
